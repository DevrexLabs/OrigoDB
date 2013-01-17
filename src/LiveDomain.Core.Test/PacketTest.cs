using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using LiveDomain.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveDomain.Core.Test
{
	[TestClass]
	public class PacketTest
	{

		[TestMethod]
		public void Packet_WriteAndRead()
		{
			var randomBytes = Guid.NewGuid().ToByteArray();
			var memoryStream = new MemoryStream();
			var packet = new Packet();

			packet.Payload = randomBytes;
			packet.Write(memoryStream);
			// Move to begining for reading.
			memoryStream.Position = 0;
			packet = new Packet();
			packet.Read(memoryStream);
			Assert.IsTrue(packet.Payload.ByteArrayCompare(randomBytes));
		}

		[TestMethod]
		public void Packet_VerifyChecksum()
		{
			var randomBytes = Guid.NewGuid().ToByteArray();
			var memoryStream = new MemoryStream();
			var packet = new Packet();

			packet.Payload = randomBytes;
			packet.IncludeChecksum = true;
			packet.Write(memoryStream);
			// Move to begining for reading.
			memoryStream.Position = 0;
			packet = new Packet();
			packet.Read(memoryStream);

			Assert.IsTrue(packet.VerifyChecksum());
			Assert.IsTrue(packet.Payload.ByteArrayCompare(randomBytes));
		}
	}
}
