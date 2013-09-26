using System.IO;
using System.IO.Compression;

namespace OrigoDB.Core.Compression
{
    public class DeflateStreamCompressor : StreamCompressor
    {
        protected override Stream CreateStream(Stream inputStream, CompressionMode compressionMode)
        {
            return new DeflateStream(inputStream, compressionMode,leaveOpen:true);
        }
    }
}