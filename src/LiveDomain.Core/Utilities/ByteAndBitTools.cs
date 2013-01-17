namespace LiveDomain.Core.Utilities
{
	public static class ByteAndBitTools
	{
		public static bool ByteArrayCompare(this byte[] a1, byte[] a2)
		{
			if (a1.Length != a2.Length)
				return false;

			for (int i = 0; i < a1.Length; i++)
				if (a1[i] != a2[i])
					return false;

			return true;
		}

		// This is not ready (Need allow unsafe on project) 
		// but about 7x faster than the simple for loop.
		// http://stackoverflow.com/a/8808245/369017
		//static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
		//{
		//    if (a1 == null || a2 == null || a1.Length != a2.Length)
		//        return false;
		//    fixed (byte* p1 = a1, p2 = a2)
		//    {
		//        byte* x1 = p1, x2 = p2;
		//        int l = a1.Length;
		//        for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
		//            if (*((long*)x1) != *((long*)x2)) return false;
		//        if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
		//        if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
		//        if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
		//        return true;
		//    }
		//}
	}
}