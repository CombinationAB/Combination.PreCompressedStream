using System;
using System.IO;
using System.IO.Compression;

namespace Combination.PreCompressedStream
{
    public sealed class PreCompressedGzipStream : PreCompressedStreamBase
    {
        private byte[] headerBytes = new byte[] {
                0x1F,
                0x8B,
                8, // Deflate
                0,
                0,
                0,
                0,
                0,
                2,
                0
            };

        public PreCompressedGzipStream(Stream inner, bool leaveOpen = false)
            : base(inner, leaveOpen)
        {
        }

        protected override void WriteHeader(Stream stream, CompressionLevel initialCompressionLevel)
        {
            switch(initialCompressionLevel)
            {
                //case CompressionLevel.Fastest:
                //    headerBytes[8] = 4;
                //    break;
                case CompressionLevel.NoCompression:
                    headerBytes[8] = 0;
                    break;
            }
            stream.Write(headerBytes, 0, 10);
        }
    }
}
