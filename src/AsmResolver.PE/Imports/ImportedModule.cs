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
using System.Threading;
using AsmResolver.PE.File;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides an implementation of the <see cref="IImportedModule"/> class, which can be instantiated and added
    /// to an existing portable executable image.
    /// </summary>
    public class ImportedModule : IImportedModule
    {
        /// <summary>
        /// Reads a single module import entry from an input stream.
        /// </summary>
        /// <param name="peFile">The PE file containing the import entry.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns></returns>
        public static IImportedModule FromReader(PEFile peFile, IBinaryStreamReader reader)
        {
            var entry = new SerializedImportedModule(peFile, reader);
            return entry.IsEmpty ? null : entry;
        }
        
        private IList<ImportedSymbol> _members;

        /// <summary>
        /// Creates a new empty module import.
        /// </summary>
        protected ImportedModule()
        {
        }

        /// <summary>
        /// Creates a new module import. 
        /// </summary>
        /// <param name="name">The name of the module to import.</param>
        public ImportedModule(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint TimeDateStamp
        {
            get;
            set;
        }

        /// <inheritdoc />
        public uint ForwarderChain
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IList<ImportedSymbol> Symbols
        {
            get
            {
                if (_members is null) 
                    Interlocked.CompareExchange(ref _members, GetSymbols(), null);
                return _members;
            }
        }

        /// <summary>
        /// Obtains the collection of members that were imported.
        /// </summary>
        /// <remarks>
        /// This method is called to initialize the value of <see cref="Symbols" /> property.
        /// </remarks>
        /// <returns>The members list.</returns>
        protected virtual IList<ImportedSymbol> GetSymbols()
        {
            return new List<ImportedSymbol>();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} ({Symbols.Count} symbols)";
        }
        
    }
}