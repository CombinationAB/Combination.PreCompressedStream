using Combination.PreCompressedStream;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xunit;

namespace PreCompressedStream.Tests
{
    public class GzipTests : TestBase
    {
        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("I am a compressed chunk of text")]
        public void Gzip_Roundtrip(string str)
        {
            var data = GzipCompress(Bytes(str));

            var uncompressedString = String(GzipDecompress(data));

            Assert.Equal(str, uncompressedString);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("I am a compressed chunk of text")]
        public void Gzip_Precompressed_Uncompressed_Roundtrip(string str)
        {
            var data = PreGzip(s => s.Write(Bytes(str)));

            var uncompressedString = String(GzipDecompress(data));

            Assert.Equal(str, uncompressedString);
        }
        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("I am a compressed chunk of text")]
        public void Gzip_Precompressed_Combine_Compression_Levels(string str)
        {
            var data = PreGzip(s =>
            {
                s.CompressionLevel = CompressionLevel.Optimal;
                s.Write(Bytes(str));
                s.CompressionLevel = CompressionLevel.NoCompression;
                s.Write(Bytes(str));
            });

            var uncompressedString = String(GzipDecompress(data));

            Assert.Equal(str + str, uncompressedString);
        }
        [Theory]
        [InlineData(0, CompressionLevel.Optimal)]
        [InlineData(0, CompressionLevel.Fastest)]
        [InlineData(0, CompressionLevel.NoCompression)]
        [InlineData(1, CompressionLevel.Optimal)]
        [InlineData(1, CompressionLevel.Fastest)]
        [InlineData(1, CompressionLevel.NoCompression)]
        [InlineData(2, CompressionLevel.Optimal)]
        [InlineData(2, CompressionLevel.Fastest)]
        [InlineData(2, CompressionLevel.NoCompression)]
        [InlineData(10, CompressionLevel.Optimal)]
        [InlineData(10, CompressionLevel.Fastest)]
        [InlineData(10, CompressionLevel.NoCompression)]
        public void Gzip_Precompressed_Roundtrip(int times, CompressionLevel level)
        {
            var data = Bytes("I am a compressed chunk of text");
            var compressedData = Precompress(data, level);

            var returnedData = PreGzip(s =>
            {
                for (var i = 0; i < times; ++i)
                    s.WritePreCompressed(compressedData);
            });

            var expected = Enumerable.Range(0, times).Select(_ => data).SelectMany(x => x).ToArray();

            var uncompressedData = GzipDecompress(returnedData);

            Assert.Equal(expected, uncompressedData);
        }
        [Theory]
        [InlineData(CompressionLevel.Optimal)]
        [InlineData(CompressionLevel.Fastest)]
        [InlineData(CompressionLevel.NoCompression)]
        public void Gzip_Precompressed_Same_As_Write(CompressionLevel level)
        {
            var data = Bytes("I am a compressed chunk of text");

            var ms = new MemoryStream();
            using (var s = new PreCompressedGzipStream(ms, true))
            {
                s.CompressionLevel = level;
                s.Write(data);
            }
            var expected = PreGzip(s =>
            {
                var compressedData = Precompress(data, level);
                s.WritePreCompressed(compressedData);
            });

            var returnedData = ms.ToArray();

            Assert.Equal(expected, returnedData);
        }
        [Theory]
        [InlineData(CompressionLevel.Optimal)]
        [InlineData(CompressionLevel.Fastest)]
        [InlineData(CompressionLevel.NoCompression)]
        public void Gzip_Precompressed_Same_As_Write_Switch_CompressionLevel(CompressionLevel startLevel)
        {
            var data = Bytes("I am a compressed chunk of text");

            var ms = new MemoryStream();
            using (var s = new PreCompressedGzipStream(ms, true))
            {
                for (var i = 0; i < 3; ++i)
                {
                    s.CompressionLevel = (CompressionLevel)(((int)startLevel + i) % 3);
                    s.Write(data);
                }
            }
            var expected = PreGzip(s =>
            {
                for (var i = 0; i < 3; ++i)
                {
                    var compressedData = Precompress(data, (CompressionLevel)(((int)startLevel + i) % 3));
                    s.WritePreCompressed(compressedData);
                }
            });

            var returnedData = ms.ToArray();

            Assert.Equal(expected, returnedData);
        }
        [Theory]
        [InlineData(CompressionLevel.Optimal)]
        [InlineData(CompressionLevel.Fastest)]
        [InlineData(CompressionLevel.NoCompression)]
        public void Gzip_Precompressed_Roundtrip_Different(CompressionLevel startLevel)
        {
            var data = Bytes("I am a compressed chunk of text");

            var returnedData = PreGzip(s =>
            {
                for (var i = 0; i < 3; ++i)
                {
                    var compressedData = Precompress(data, (CompressionLevel)(((int)startLevel + i) % 3));
                    s.WritePreCompressed(compressedData);
                }
            });

            var expected = Enumerable.Range(0, 3).Select(_ => data).SelectMany(x => x).ToArray();

            var uncompressedData = GzipDecompress(returnedData);

            Assert.Equal(expected, uncompressedData);
        }
    }
}
