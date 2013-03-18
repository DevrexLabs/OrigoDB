using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LiveDomain.Modules.ProtoBuf;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;
using Modules.ProtoBuf.Test.Framework;
using Modules.ProtoBuf.Test.Domain;

namespace Modules.ProtoBuf.Test
{
    [TestClass]
    public class ProtoBufFormatterTests
    {
        [TestMethod]
        public void FormatterHasStreamingContext()
        {
            var formatter = new ProtoBufFormatter();
            Assert.IsNotNull(formatter.Context);
        }

        [TestMethod]
        public void SerializationContextStateIsSetToPersistence()
        {
            var formatter = new ProtoBufFormatter();
            Assert.AreEqual(StreamingContextStates.Persistence, formatter.Context.State);
        }


        [TestMethod]
        public void CanDeserializeType()
        {
            // Arrange
            var formatter = new ProtoBufFormatter();
            var graph = new Employee(16) { Name = "Kalle", Age = 42 };

            // Act
            var stream = SerializationHelper.Serialize<Employee>(graph, formatter);
            var result = SerializationHelper.Deserialize<Employee>(stream, formatter);

            // Act
            Assert.IsInstanceOfType(result, typeof(Employee));
            Assert.AreEqual("Kalle", result.Name);
            Assert.AreEqual(42, result.Age);
            Assert.AreEqual(16,result.ShoeSize);
        }

        [TestMethod]
        public void CanDeserializeComplexType()
        {
            // Arrange
            var formatter = new ProtoBufFormatter();
            var graph = new Company() {
                Name = "Initech Corporation",
                Employees = new List<Employee> {
                        new Employee() { Name = "Peter Gibbons", Age = 34 },
                        new Employee() { Name = "Michael Bolton", Age = 39 }
                    }
            };

            // Act
            var stream = SerializationHelper.Serialize<Company>(graph, formatter);
            var result = SerializationHelper.Deserialize<Company>(stream, formatter);

            // Act
            Assert.AreEqual("Initech Corporation", result.Name);
            Assert.IsNotNull(result.Employees);
            Assert.AreEqual(2, result.Employees.Count);
            Assert.AreEqual("Peter Gibbons", result.Employees.ElementAt(0).Name);
            Assert.AreEqual("Michael Bolton", result.Employees.ElementAt(1).Name);
        }

        [TestMethod]
        public void NonSerializedTypeIsNotConsideredKnown()
        {
            var formatter = new ProtoBufFormatter();
            Assert.IsFalse(formatter.IsKnownType(typeof(Employee)));
        }

        [TestMethod]
        public void SerializedTypeIsConsideredKnown()
        {
            var formatter = new ProtoBufFormatter();
            var graph = new Employee();
            var stream = SerializationHelper.Serialize<Employee>(graph, formatter);
            Assert.IsTrue(formatter.IsKnownType(typeof(Employee)));
        }
    }
}
