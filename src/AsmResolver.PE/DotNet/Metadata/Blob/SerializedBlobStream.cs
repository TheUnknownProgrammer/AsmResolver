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

namespace AsmResolver.PE.DotNet.Metadata.Blob
{
    /// <summary>
    /// Provides an implementation of a blob stream that obtains blobs from a readable segment in a file.  
    /// </summary>
    public class SerializedBlobStream : BlobStream
    {
        private readonly IReadableSegment _contents;

        /// <summary>
        /// Creates a new blob stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedBlobStream(string name, byte[] rawData)
            : this(name, new DataSegment(rawData))
        {
        }

        /// <summary>
        /// Creates a new blob stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="contents">The raw contents of the stream.</param>
        public SerializedBlobStream(string name, IReadableSegment contents)
            : base(name)
        {
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override IBinaryStreamReader CreateReader() => _contents.CreateReader();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _contents.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _contents.Write(writer);

        /// <inheritdoc />
        public override byte[] GetBlobByIndex(uint index) => GetBlobReaderByIndex(index)?.ReadToEnd();

        /// <inheritdoc />
        public override IBinaryStreamReader GetBlobReaderByIndex(uint index)
        {
            if (index == 0 || index >= _contents.GetPhysicalSize()) 
                return null;
            
            var blobReader = _contents.CreateReader((uint) (_contents.FileOffset + index));
            if (blobReader.TryReadCompressedUInt32(out uint length))
            {
                uint headerSize = blobReader.FileOffset - blobReader.StartPosition;
                blobReader.ChangeSize(Math.Min(length + headerSize, blobReader.Length));
                return blobReader;
            }

            return null;
        }
        
    }
}