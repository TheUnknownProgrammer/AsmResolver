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

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the event map metadata table.
    /// </summary>
    public readonly struct EventMapRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single event map row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the event map table.</param>
        /// <returns>The row.</returns>
        public static EventMapRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new EventMapRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        public EventMapRow(uint parent, uint eventList)
        {
            Parent = parent;
            EventList = eventList;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.EventMap;

        /// <summary>
        /// Gets an index into the TypeDef table that this mapping is associating to an event list.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Event table indicating the first event that is defined in the event list.
        /// </summary>
        public uint EventList
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided event map row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(EventMapRow other)
        {
            return Parent == other.Parent && EventList == other.EventList;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is EventMapRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Parent * 397) ^ (int) EventList;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Parent:X8}, {EventList:X8})";
        }
        
    }
}