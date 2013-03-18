using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.IO;
using Lzf;

namespace OrigoDB.Core.Compression
{
    public interface ICompressor
    {
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }

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

    public class DeflateStreamCompressor : StreamCompressor
    {
        protected override Stream CreateStream(Stream inputStream, CompressionMode compressionMode)
        {
            return new DeflateStream(inputStream, compressionMode,leaveOpen:true);
        }
    }

    public class GzipCompressor : StreamCompressor
    {
        protected override Stream CreateStream(Stream inputStream, CompressionMode compressionMode)
        {
            return  new GZipStream(inputStream, compressionMode, leaveOpen:true);
        }
    }

    public class LzfCompressionAdapter : ICompressor
    {
        LZF lzf = new LZF();
        public byte[] Compress(byte[] data)
        {
            byte[] resultBuffer = new byte[data.Length + 1000];
            int compressedSize = lzf.Compress(data, data.Length, resultBuffer, resultBuffer.Length);
            byte[] result = new byte[compressedSize];
            for (int i = 0; i < compressedSize; i++)
            {
                result[i] = resultBuffer[i];
            }
            return result;            
        }

        public byte[] Decompress(byte[] data)
        {
            byte[] resultBuffer = new byte[data.Length * 20];
            int resultSize = lzf.Decompress(data, data.Length, resultBuffer, resultBuffer.Length);
            byte[] result = new byte[resultSize];
            for (int i = 0; i < resultSize; i++)
            {
                result[i] = resultBuffer[i];
            }
            return result;
        }
    }

}
