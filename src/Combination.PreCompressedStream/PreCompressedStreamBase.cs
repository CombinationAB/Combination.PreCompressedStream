using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Combination.PreCompressedStream
{
    public abstract class PreCompressedStreamBase : Stream
    {
        private readonly Stream inner;
        private Stream innerCompressed;
        private readonly bool leaveOpen;
        private bool didWriteHeader;
        private CompressionLevel compressionLevel = CompressionLevel.Optimal, lastUsedCompressionLevel = CompressionLevel.Optimal;
        private static readonly byte[] deflateFooter = new byte[] { 0, 0, 0xff, 0xff, 3, 0 };
        private static readonly byte[] uncompressedFooter = new byte[] { 0, 0, 0xff, 0xff, 1, 0, 0, 0xff, 0xff };
        

        internal PreCompressedStreamBase(Stream inner, bool leaveOpen)
        {
            this.inner = inner;
            this.leaveOpen = leaveOpen;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => inner.CanWrite;

        public override long Length => inner.Length;

        public override long Position { get => throw new InvalidOperationException("Stream is not seekable"); set => throw new InvalidOperationException("Stream is not seekable"); }

        public CompressionLevel CompressionLevel
        {
            get => compressionLevel;
            set
            {
                EndWriteUncompressed();
                lastUsedCompressionLevel = compressionLevel = value;
            }
        }

        public override void Flush()
        {
            innerCompressed?.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Stream is not readable");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException("Stream is not seekable");
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Stream is not seekable");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0) return;
            BeginWriteUncompressed();
            innerCompressed.Write(buffer, offset, count);
        }


        /// <summary>
        /// Writes an already compressed chunk to the stream. The data must have been compressed using the PreCompressionStream class.
        /// </summary>
        /// <param name="buffer">The buffer of pre-compressed data.</param>
        public void WritePreCompressed(byte[] buffer)
        {
            WritePreCompressed(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes an already compressed chunk to the stream. The data must have been compressed using the PreCompressionStream class.
        /// </summary>
        /// <param name="buffer">The buffer of pre-compressed data.</param>
        /// <param name="offset">Offset at which to start reading.</param>
        /// <param name="count">Number of bytes in the buffer to write.</param>
        public void WritePreCompressed(byte[] buffer, int offset, int count)
        {
            CompressionLevel level;
            if (count >= uncompressedFooter.Length && uncompressedFooter.SequenceEqual(buffer.Skip(offset + count - uncompressedFooter.Length)))
            {
                count -= 5;
                level = CompressionLevel.NoCompression;
            }
            else if (count >= deflateFooter.Length && deflateFooter.SequenceEqual(buffer.Skip(offset + count - deflateFooter.Length)))
            {
                count -= 2;
                level = CompressionLevel.Optimal;
            }
            else
            {
                throw new ArgumentException("Block is not compressed using PreCompressedStream");
            }
            lastUsedCompressionLevel = level;
            BeginWritePreCompressed(level);
            inner.Write(buffer, offset, count);
        }

        private CompressionLevel CheckGzipHeader(byte[] buffer, int offset, int count)
        {
            if (count >= 10 && buffer[offset] == 0x1F && buffer[offset + 1] == 0x8B)
            {
                if (buffer[offset + 2] != 8) throw new FormatException("Only deflate-compressed chunks are supported");
                if (buffer[offset + 8] == 2)
                {
                    return CompressionLevel.Optimal;
                }
                else if (buffer[offset + 8] == 4)
                {
                    return CompressionLevel.Fastest;
                }
                else
                {
                    throw new FormatException("Unsupported compression level");
                }
            }
            return CompressionLevel.Optimal;
        }

        private void BeginWritePreCompressed(CompressionLevel level)
        {
            EndWriteUncompressed();
            if (!didWriteHeader)
            {
                didWriteHeader = true;
                WriteHeader(inner, level);
            }
        }

        private void BeginWriteUncompressed()
        {
            if (!didWriteHeader)
            {
                didWriteHeader = true;
                WriteHeader(inner, compressionLevel);
            }
            if (innerCompressed == null)
            {
                innerCompressed = new DeflateStream(inner, compressionLevel, true);
            }
        }

        private void EndWriteUncompressed()
        {
            if (innerCompressed != null)
            {
                innerCompressed.Flush();
                // It is the intent not to dispose the stream here, since we
                // don't want the end of stream block to be written at this point
                innerCompressed = null;
            }
        }

        protected virtual void WriteHeader(Stream stream, CompressionLevel initialCompressionLevel)
        {
        }

        protected virtual void WriteFooter(Stream stream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EndWriteUncompressed();
                if (didWriteHeader)
                {
                    if (lastUsedCompressionLevel == CompressionLevel.NoCompression)
                    {
                        inner.Write(uncompressedFooter, 4, 5);
                    }
                    else
                    {
                        inner.Write(deflateFooter, 4, 2);
                    }
                    WriteFooter(inner);
                }
                if (!leaveOpen) inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
