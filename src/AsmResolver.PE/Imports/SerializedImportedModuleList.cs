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
using System.Diagnostics;
using AsmResolver.Collections;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides a lazy-initialized list of module import entries that is stored in a PE file.
    /// </summary>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class SerializedImportedModuleList : LazyList<IImportedModule>
    {
        private readonly PEFile _peFile;
        private readonly DataDirectory _dataDirectory;

        /// <summary>
        /// Prepares a new lazy-initialized list of module import entries.
        /// </summary>
        /// <param name="peFile">The PE file containing the list of modules.</param>
        /// <param name="dataDirectory">The import data directory.</param>
        public SerializedImportedModuleList(PEFile peFile, DataDirectory dataDirectory)
        {
            _peFile = peFile ?? throw new ArgumentNullException(nameof(peFile));
            _dataDirectory = dataDirectory;
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            if (!_peFile.TryCreateDataDirectoryReader(_dataDirectory, out var reader))
                return;
            
            while (true)
            {
                var entry = ImportedModule.FromReader(_peFile, reader);
                if (entry == null)
                    break;
                Items.Add(entry);
            }
        }
        
    }
}