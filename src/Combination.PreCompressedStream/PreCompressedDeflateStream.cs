using System;
using System.IO;
using System.IO.Compression;

namespace Combination.PreCompressedStream
{
    public sealed class PreCompressedDeflateStream : PreCompressedStreamBase
    {
        public PreCompressedDeflateStream(Stream inner, bool leaveOpen = false)
            : base(inner, leaveOpen)
        {
        }
    }
}
