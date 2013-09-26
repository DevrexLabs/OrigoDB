using System.IO;
using System.IO.Compression;

namespace OrigoDB.Core.Compression
{
    public abstract class StreamCompressor : ICompressor
    {
        protected abstract Stream CreateStream(Stream stream, CompressionMode compressionMode);

        public byte[] Compress(byte[] data)
        {
            var inputStream = new MemoryStream(data, writable:false);
            var outputStream = new MemoryStream(data.Length);
            var compressionStream = CreateStream(outputStream, CompressionMode.Compress);
            inputStream.CopyTo(compressionStream);
            compressionStream.Dispose();
            return outputStream.ToArray();
        }

        public byte[] Decompress(byte[] data)
        {
            var inputStream = new MemoryStream(data);
            var outputStream = new MemoryStream(data.Length * 2);
            var compressionStream = CreateStream(inputStream, CompressionMode.Decompress);
            compressionStream.CopyTo(outputStream);
            compressionStream.Close();
            return outputStream.ToArray();
        }
    }
}