// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Win32Resources
{
    /// <summary>
    /// Provides an implementation of a resource directory that was read from an existing PE file.
    /// </summary>
    public class SerializedResourceDirectory : ResourceDirectory
    {
        /// <summary>
        /// Indicates the size of a single sub-directory entry in a resource directory.
        /// </summary>
        public const uint ResourceDirectorySize = 2 * sizeof(uint) + 4 * sizeof(ushort); 
            
        /// <summary>
        /// Indicates the maximum depth of sub directories a resource directory can have before AsmResolver aborts
        /// reading the resource tree branch. 
        /// </summary>
        public const int MaxDepth = 10;
            
        private readonly PEFile _peFile;
        private readonly IWin32ResourceDataReader _dataReader;
        private readonly ushort _namedEntries;
        private readonly ushort _idEntries;
        private readonly uint _entriesOffset;
        private readonly int _depth;

        /// <summary>
        /// Reads a single resource directory from an input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the resource.</param>
        /// <param name="dataReader">The instance responsible for reading and interpreting the data.</param>
        /// <param name="entry">The entry to read. If this value is <c>null</c>, the root directory is assumed.</param>
        /// <param name="directoryReader">The input stream.</param>
        /// <param name="depth">
        /// The current depth of the resource directory tree structure.
        /// If this value exceeds <see cref="MaxDepth"/>, this class will not initialize any entries.
        /// </param>
        public SerializedResourceDirectory(PEFile peFile, IWin32ResourceDataReader dataReader,
            ResourceDirectoryEntry? entry, IBinaryStreamReader directoryReader, int depth = 0)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));

            _depth = depth;

            if (entry.HasValue)
            {
                var value = entry.Value;
                if (value.IsByName)
                    Name = value.Name;
                else
                    Id = value.IdOrNameOffset;
            }

            if (directoryReader != null)
            {
                Characteristics = directoryReader.ReadUInt32();
                TimeDateStamp = directoryReader.ReadUInt32();
                MajorVersion = directoryReader.ReadUInt16();
                MinorVersion = directoryReader.ReadUInt16();

                _namedEntries = directoryReader.ReadUInt16();
                _idEntries = directoryReader.ReadUInt16();
                _entriesOffset = directoryReader.FileOffset;

                directoryReader.FileOffset =
                    (uint) (directoryReader.FileOffset + (_namedEntries + _idEntries) * ResourceDirectoryEntry.EntrySize);
            }
        }

        /// <inheritdoc />
        protected override IList<IResourceEntry> GetEntries()
        {
            var result = new OwnedCollection<IResourceDirectory, IResourceEntry>(this);
            
            // Optimisation, check for invalid resource directory offset, and prevention of self loop:
            if (_namedEntries + _idEntries == 0 || _depth >= MaxDepth)
                return result;

            uint baseRva = _peFile.OptionalHeader.DataDirectories[OptionalHeader.ResourceDirectoryIndex].VirtualAddress;

            // Create entries reader.
            uint entryListSize = (uint) ((_namedEntries + _idEntries) * ResourceDirectoryEntry.EntrySize);
            var entriesReader = _peFile.CreateReaderAtFileOffset(_entriesOffset, entryListSize);

            for (int i = 0; i < _namedEntries + _idEntries; i++)
            {
                var rawEntry = new ResourceDirectoryEntry(_peFile, entriesReader);
                
                // Note: Even if creating the directory reader fails, we still want to include the directory entry
                //       itself. In such a case, we expose the directory as an empty directory. This is why the
                //       following statement is not used as a condition for an if statement.
                
                _peFile.TryCreateReaderAtRva(baseRva + rawEntry.DataOrSubDirOffset, out var entryReader);
                
                result.Add(rawEntry.IsSubDirectory
                    ? (IResourceEntry) new SerializedResourceDirectory(_peFile, _dataReader, rawEntry, entryReader, _depth + 1)
                    : new SerializedResourceData(_peFile, _dataReader, rawEntry, entryReader));
            }

            return result;
        }

    }
}