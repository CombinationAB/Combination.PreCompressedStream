# Combination.PreCompressedStream

## Combine already compressed streams

This library was created to solve the problem of preserving (caching, storing) individual compressed entries and then wanting to archive or serve
a combination of these entries without recompressing.

The library originated out of .NET core's inability to work with concatenated GzipStreams (see https://github.com/dotnet/corefx/issues/27279), but 
can be useful even post .NET core 3.0, since compatibility with all clients is desired in web applications.

This libary produces Deflate or GZip encoded data that should be compliant and compatible, but at the expense of a few bytes lost for each item (in
addition to the cost of doing a full flush of the encode stream).

As a side effect, the streams provided by this library can also change compression level on the fly (at the expense of a full flush).

This library provides drop-in replacements to `GzipStream` (`PreCompressedGzipStream`) and `DeflateStream` (`PreCompressedDeflateStream`) that adds 
the `WritePreCompressed` method to write already compressed data to the stream.

## Limitations

A limitation of this library (due to the inherent limitation in the Deflate stream format) is that pre-compressed data must be compressed using the `PreCompressionStream` class.
Data compressed using normal deflate will not work (unless you manually do a full flush before disposing the stream). Otherwise finding the end of stream block would require a full
decompression of the stream (which is currently not done). If the data is not compatible, `WritePreCompressed` with throw an exception.

Another limitation (due to lack of time) is that pre-compressed data is only supported as byte arrays and hence need to reside in memory. It would be relatively easy to support
pre-compressed data in the form of streams.
