using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDomain.Core.Utilities
{
	public static class ByteArrayExtensions
	{
		public static bool EqualsEx(this byte[] a1, byte[] a2)
		{
			return a1.Length == a2.Length 
                && Enumerable.Range(0, a1.Length).AsParallel().All(i => a1[i] == a2[i]);
		}

	    public static byte[] Compress(this byte[] data)
	    {
	        var inputStream = new MemoryStream(data, false);
	        var outputStream = new MemoryStream(data.Length);
	        var zipStream = new GZipStream(outputStream, CompressionMode.Compress);
	        inputStream.CopyTo(zipStream);
	        zipStream.Dispose();
	        return outputStream.ToArray();
	    }

	    public static byte[] Decompress(this byte[] data)
	    {
	        var inputStream = new MemoryStream(data);
	        var outputStream = new MemoryStream(data.Length * 2);
	        var zipStream = new GZipStream(inputStream, CompressionMode.Decompress);
	        zipStream.CopyTo(outputStream);
	        zipStream.Close();
	        return outputStream.ToArray();
	    }
	}
}