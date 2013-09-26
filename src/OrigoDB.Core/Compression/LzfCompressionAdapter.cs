using Lzf;

namespace OrigoDB.Core.Compression
{
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