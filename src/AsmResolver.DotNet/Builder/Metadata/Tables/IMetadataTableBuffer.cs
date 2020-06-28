﻿using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Metadata.Tables
{
    /// <summary>
    /// Provides members for constructing a new metadata table.
    /// </summary>
    public interface IMetadataTableBuffer
    {
        /// <summary>
        /// Gets the number of rows that were added to the buffer.
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Submits all rows to the underlying table stream.
        /// </summary>
        void FlushToTable();

        /// <summary>
        /// Clears the table buffer.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Provides members for constructing a new metadata table.
    /// </summary>
    /// <typeparam name="TRow">The type of rows the table stores.</typeparam>
    public interface IMetadataTableBuffer<TRow> : IMetadataTableBuffer
        where TRow : struct, IMetadataRow
    {
        /// <summary>
        /// Gets or sets a row in the metadata table.
        /// </summary>
        /// <param name="rid">The identifier of the metadata row.</param>
        TRow this[uint rid]
        {
            get;
            set;
        }

        /// <summary>
        /// Adds a row to the metadata table.
        /// </summary>
        /// <param name="row">The row to add.</param>
        /// <param name="originalRid">The original identifier of the row (RID).</param>
        /// <returns>The metadata token that this row was assigned to.</returns>
        /// <remarks>
        /// For some metadata table buffers, the metadata token that the row was assigned to might not be definitive. Sorted
        /// metadata table buffers will reorder the table once all rows have been added to the buffer.
        /// </remarks>
        MetadataToken Add(in TRow row, uint originalRid);
    }
}