using Combination.PreCompressedStream;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xunit;

namespace PreCompressedStream.Tests
{
    public class PreCompressionStreamTests : TestBase
    {
        [Theory]
        [InlineData("", CompressionLevel.Optimal, 1)]
        [InlineData("", CompressionLevel.Optimal, 10)]
        [InlineData("", CompressionLevel.Fastest, 1)]
        [InlineData("", CompressionLevel.NoCompression, 1)]
        [InlineData("a", CompressionLevel.Optimal, 1)]
        [InlineData("a", CompressionLevel.Optimal, 10)]
        [InlineData("a", CompressionLevel.Fastest, 1)]
        [InlineData("a", CompressionLevel.NoCompression, 1)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Optimal, 1)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Optimal, 10)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Fastest, 1)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.NoCompression, 1)]
        public void Precompress_Roundtrip(string str, CompressionLevel level, int multiply)
        {
            var data = Precompress(Bytes(str), level, multiply);

            var uncompressedString = String(DeflateDecompress(data));

            Assert.Equal(new string(Enumerable.Range(0, multiply).Select(_ => str).SelectMany(x => x).ToArray()), uncompressedString);
        }
        [Theory]
        [InlineData("", CompressionLevel.Optimal, 1)]
        [InlineData("", CompressionLevel.Optimal, 10)]
        [InlineData("", CompressionLevel.Fastest, 1)]
        [InlineData("a", CompressionLevel.Optimal, 1)]
        [InlineData("a", CompressionLevel.Optimal, 10)]
        [InlineData("a", CompressionLevel.Fastest, 1)]
        [InlineData("a", CompressionLevel.Fastest, 10)]
        [InlineData("a", CompressionLevel.Fastest, 100)]
        [InlineData("a", CompressionLevel.Fastest, 1000)]
        [InlineData("a", CompressionLevel.Fastest, 1001)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Optimal, 1)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Optimal, 10)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Optimal, 11)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Fastest, 1)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Fastest, 10)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.Fastest, 11)]
        public void Precompress_Always_Produces_Compressed_Trailer(string str, CompressionLevel level, int multiply)
        {
            var data = Precompress(Bytes(str), level, multiply);

            Assert.Equal(new byte[] { 0, 0, 0xff, 0xff, 3, 0 }, data.Skip(data.Length - 6).ToArray());
        }
        [Theory]
        [InlineData("", CompressionLevel.NoCompression, 1)]
        [InlineData("", CompressionLevel.NoCompression, 10)]
        [InlineData("a", CompressionLevel.NoCompression, 1)]
        [InlineData("a", CompressionLevel.NoCompression, 10)]
        [InlineData("a", CompressionLevel.NoCompression, 100)]
        [InlineData("a", CompressionLevel.NoCompression, 101)]
        [InlineData("a", CompressionLevel.NoCompression, 1000)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.NoCompression, 1)]
        [InlineData("I am a compressed chunk of text", CompressionLevel.NoCompression, 10)]
        public void Precompress_Always_Produces_Uncompressed_Trailer(string str, CompressionLevel level, int multiply)
        {
            var data = Precompress(Bytes(str), level, multiply);

            Assert.Equal(new byte[] { 0, 0, 0xff, 0xff, 1, 0, 0, 0xff, 0xff }, data.Skip(data.Length - 9).ToArray());
        }
    }
}
