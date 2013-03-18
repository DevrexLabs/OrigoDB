using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrigoDB.Modules.ProtoBuf;
using System.IO;
using Modules.ProtoBuf.Test.Framework;
using Modules.ProtoBuf.Test.Domain;

namespace Modules.ProtoBuf.Test
{
    [TestClass]
    public class ProtoBufStreamHeaderTests
    {
        [TestMethod]
        public void HeaderReadFromStreamContainsCorrectLength()
        {
            var stream = SerializationHelper.Serialize<Employee>(new Employee());
            var expectedHeader = ProtoBufStreamHeader.Create(typeof(Employee));
            var header = ProtoBufStreamHeader.Read(stream);

            // Assert
            Assert.AreEqual(expectedHeader.Length, header.Length);
        }

        [TestMethod]
        public void HeaderReadFromStreamContainsCorrectTypeName()
        {
            var stream = SerializationHelper.Serialize<Employee>(new Employee());
            var expectedHeader = ProtoBufStreamHeader.Create(typeof(Employee));
            var header = ProtoBufStreamHeader.Read(stream);

            // Assert
            Assert.AreEqual(expectedHeader.TypeName, header.TypeName);
        }

        [TestMethod]
        public void HeaderReadFromStreamContainsCorrectBuffer()
        {
            var stream = SerializationHelper.Serialize<Employee>(new Employee());
            var expectedHeader = ProtoBufStreamHeader.Create(typeof(Employee));
            var header = ProtoBufStreamHeader.Read(stream);

            // Assert
            CollectionAssert.AreEqual(expectedHeader.Buffer, header.Buffer);
        }
    }
}
