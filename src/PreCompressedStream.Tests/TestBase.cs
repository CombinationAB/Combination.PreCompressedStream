using Combination.PreCompressedStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PreCompressedStream.Tests
{
    public abstract class TestBase
    {

        protected static byte[] Bytes(string str) => Encoding.UTF8.GetBytes(str);
        protected static string String(byte[] data) => Encoding.UTF8.GetString(data);
        protected static byte[] GzipCompress(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal, int multiply = 1)
        {
            var gzipBytes = new MemoryStream();

            using (var gzipStream = new GZipStream(gzipBytes, compressionLevel, true))
            {
                for (var i = 0; i < multiply; ++i)
                    gzipStream.Write(data);

            }
            return gzipBytes.ToArray();
        }
        protected static byte[] DeflateCompress(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal, int multiply = 1)
        {
            var deflateBytes = new MemoryStream();

            using (var deflateStream = new DeflateStream(deflateBytes, compressionLevel, true))
            {
                for (var i = 0; i < multiply; ++i)
                    deflateStream.Write(data);

            }
            return deflateBytes.ToArray();
        }
        protected static byte[] Precompress(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal, int multiply = 1)
        {
            var deflateBytes = new MemoryStream();

            using (var deflateStream = new PreCompressionStream(deflateBytes, compressionLevel, true))
            {
                for (var i = 0; i < multiply; ++i)
                    deflateStream.Write(data);

            }
            return deflateBytes.ToArray();
        }
        protected static byte[] PreGzip(Action<PreCompressedGzipStream> writer)
        {
            var gzipBytes = new MemoryStream();

            using (var gzipStream = new PreCompressedGzipStream(gzipBytes, true))
            {
                writer(gzipStream);
            }
            return gzipBytes.ToArray();
        }
        protected static byte[] PreDeflate(Action<PreCompressedDeflateStream> writer)
        {
            var deflateBytes = new MemoryStream();

            using (var deflateStream = new PreCompressedDeflateStream(deflateBytes, true))
            {
                writer(deflateStream);
            }
            return deflateBytes.ToArray();
        }
        protected static byte[] GzipDecompress(byte[] data)
        {
            var rawBytes = new MemoryStream();

            using (var gzipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
            {
                gzipStream.CopyTo(rawBytes);
                return rawBytes.ToArray();
            }
        }
        protected static byte[] DeflateDecompress(byte[] data)
        {
            var rawBytes = new MemoryStream();

            using (var deflateStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
            {
                deflateStream.CopyTo(rawBytes);
                return rawBytes.ToArray();
            }
        }

        protected static byte[] Bits(string bitstring)
        {
            var a = new List<byte>();
            var bit = 0;
            byte b = 0;
            foreach(var c in bitstring)
            {
                if(c == '0')
                {
                    ++bit;
                } else if(c == '1')
                {
                    b |= (byte)(1 << bit);
                    ++bit;
                }
                if(bit == 8)
                {
                    a.Add(b);
                    bit = 0;
                    b = 0;
                }
            }
            if (bit > 0) a.Add(b);
            return a.ToArray();
        }
    }
}
