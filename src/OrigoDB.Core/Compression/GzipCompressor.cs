using System.IO;
using System.IO.Compression;

namespace OrigoDB.Core.Compression
{
    public class GzipCompressor : StreamCompressor
    {
        protected override Stream CreateStream(Stream inputStream, CompressionMode compressionMode)
        {
            return  new GZipStream(inputStream, compressionMode, leaveOpen:true);
        }
    }
}