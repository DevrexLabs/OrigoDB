using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using OrigoDB.Core.Compression;

namespace OrigoDB.Core.Utilities
{
    public enum CompressionAlgorithm
    {
        Gzip,
        Deflate,
        Lz4
    }

	public static class ByteArrayExtensions
	{

	    public static ICompressor CompressionAlgorithm = new DeflateStreamCompressor();

		public static bool EqualsEx(this byte[] a1, byte[] a2)
		{
			return a1.Length == a2.Length 
                && Enumerable.Range(0, a1.Length).AsParallel().All(i => a1[i] == a2[i]);
		}


        public static byte[] Compress(this byte[] data)
        {
            return CompressionAlgorithm.Compress(data);
        }

        public static byte[] Decompress(this byte[] data)
        {
            return CompressionAlgorithm.Decompress(data);
        }
	}
}