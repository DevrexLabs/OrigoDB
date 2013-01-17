using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LiveDomain.Core.Utilities
{
	public class Packet
	{
		/** Header is one byte in information bits(8 bools)
		 *	0 IsEncrypted?
		 *	1 IsCompressed?
		 *	2 IncludeChecksum? 
		 *	3
		 *	4
		 *	5
		 *	6
		 *	7
		 *	
		 *	Next four bytes is the length of payload data in Int32 format 
		 *		followed by the corresponding length of payload data.
		 *	
		 *  IF IncludeChecksum : Next four bytes is the length of checksum data in Int32 format 
		 *		followed by the corresponding  length of checksum data.
		 **/

		HashAlgorithm _hasher;
		BitArray _headerBits = new BitArray(8);
		byte[] _checksum;
		byte[] _payload;

		public Packet()
		{
			_hasher = new MD5CryptoServiceProvider();
			_headerBits.SetAll(false);
		}

		public bool IsEncrypted
		{
			get { return _headerBits[0]; }
			set { _headerBits[0] = value; }
		}

		public bool IsCompressed
		{
			get { return _headerBits[1]; }
			set { _headerBits[1] = value; }
		}

		public bool IncludeChecksum
		{
			get { return _headerBits[2]; }
			set { _headerBits[2] = value; }
		}


		public byte[] Payload
		{
			get { return _payload; }
			set { _payload = value; }
		}

		public void Read(Stream stream)
		{
			// Read header
			var headerByte = (byte)stream.ReadByte();
			_headerBits = new BitArray(new byte[] { headerByte });

			// Read Payload
			Read(stream,out _payload);

			if (IncludeChecksum)
				Read(stream, out _checksum);
		}

		void Read(Stream stream, out byte[] data)
		{
			var lengthInBytes = new byte[4];
			stream.Read(lengthInBytes, 0, lengthInBytes.Length);
			var lengthToRead = BitConverter.ToInt32(lengthInBytes, 0);
			data = new byte[lengthToRead];
			stream.Read(data, 0, data.Length);
		}

		public void Write(Stream stream)
		{
			// Write header
			var headerBytes = new byte[1];
			_headerBits.CopyTo(headerBytes, 0);
			stream.Write(headerBytes, 0, headerBytes.Length);
			
			Write(stream,Payload);

			if (IncludeChecksum)
			{
				_checksum = _hasher.ComputeHash(Payload);
				Write(stream, _checksum);
			}
		}

		void Write(Stream stream,byte[] bytes)
		{
			var lengthInBytes = BitConverter.GetBytes(bytes.Length);
			stream.Write(lengthInBytes, 0, lengthInBytes.Length);
			stream.Write(bytes, 0, bytes.Length);
		}

		public bool VerifyChecksum()
		{
			if(!IncludeChecksum) return true;
			byte[] computedHash = _hasher.ComputeHash(Payload);
			return computedHash.ByteArrayCompare(_checksum);
		}
	}

	public class Packager
	{
		public static IEnumerable<Packet> ReadAll(Stream stream)
		{
			while (stream.Position < (stream.Length - 1))
			{
				var packet = new Packet();
				packet.Read(stream);
				yield return packet;
			}
		}
	}
}