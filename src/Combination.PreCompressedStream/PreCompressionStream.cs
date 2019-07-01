using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Combination.PreCompressedStream
{
    public sealed class PreCompressionStream : DeflateStream
    {
        public PreCompressionStream(Stream stream)
            : this(stream, CompressionLevel.Optimal)
        {

        }
        public PreCompressionStream(Stream stream, bool leaveOpen)
            : this(stream, CompressionLevel.Optimal, leaveOpen)
        {

        }
        public PreCompressionStream(Stream stream, CompressionLevel compressionLevel)
            : this(stream, compressionLevel, false)
        {

        }
        public PreCompressionStream(Stream stream, CompressionLevel compressionLevel, bool leaveOpen)
            : base(stream, compressionLevel, leaveOpen)
        {
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
        }
    }
}
