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
        public void Packet_with_checksum_is_same_after_writing_reading()
        {
            var randomBytes = Guid.NewGuid().ToByteArray();
            var packet = Packet.Create(randomBytes);
            var memoryStream = new MemoryStream();
            packet.Write(memoryStream);

            // Move to begining for reading.
            memoryStream.Position = 0;
            var recreatedPacket = Packet.Read(memoryStream);
            
            Assert.AreEqual(new Guid(recreatedPacket.Bytes), new Guid(randomBytes));
        }

        [TestMethod]
        public void Packet_without_checksum_is_same_after_writing_reading()
        {
            var randomBytes = Guid.NewGuid().ToByteArray();
            var packet = Packet.Create(randomBytes, PacketOptions.None);
            var memoryStream = new MemoryStream();
            packet.Write(memoryStream);

            // Move to begining for reading.
            memoryStream.Position = 0;
            var recreatedPacket = Packet.Read(memoryStream);

            Assert.IsFalse(recreatedPacket.IncludeChecksum);
            Assert.AreEqual(new Guid(recreatedPacket.Bytes), new Guid(randomBytes));
        }
	}
}
