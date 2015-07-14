using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class SerializerTest
    {
        [Test]
        public void SizeOf_reports_actual_size()
        {
            IFormatter target = new BinaryFormatter();
            var testModel = new TestModel();
            testModel.AddCustomer("Homer");
            testModel.AddCustomer("Bart");
            long actual = target.SizeOf(testModel);
            long expected = target.ToByteArray(testModel).Length;
            Console.WriteLine("SizeOf(TestModel with 2 customers) using BinaryFormatter = " + expected);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SizeOf_throws_when_passed_null_graph()
        {
            var target = new BinaryFormatter();
            target.SizeOf(null);
        }
    }
}
