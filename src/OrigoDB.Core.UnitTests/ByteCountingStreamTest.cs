using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using OrigoDB.Core.Storage;
using System;

namespace OrigoDB.Core.Test
{
    
    [TestFixture]
    public class ByteCountingStreamTest
    {

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Write_throws_when_buffer_is_null()
        {
            var target = new ByteCountingStream();
            target.Write(null, 0, 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Write_throws_when_buffer_offset_equals_length()
        {
            var target = new ByteCountingStream();
            var buffer = new byte[100];
            target.Write(buffer, 100, 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Write_throws_when_buffer_offset_and_count_exceed_length()
        {
            var target = new ByteCountingStream();
            var buffer = new byte[100];
            target.Write(buffer, 50, 51);
        }

        [Test]
        public void Write_accepts_last_byte_of_buffer()
        {
            var target = new ByteCountingStream();
            var buffer = new byte[100];
            target.Write(buffer, 99, 1);
            Assert.AreEqual(1, target.Length);
        }

        [Test]
        public void ByteCountingStream_count_tests()
        {
            var data = Enumerable.Range(1, 1000).ToList();
            var formatter = new BinaryFormatter();
            var memStream = new MemoryStream();
            var target = new ByteCountingStream(memStream);
            formatter.Serialize(target, data);
            Assert.AreEqual(target.Length, memStream.Length);
            memStream.Position = 0;
            var clone = (List<int>) formatter.Deserialize(memStream);
            CollectionAssert.AreEqual(data, clone);
        }


    }
}
