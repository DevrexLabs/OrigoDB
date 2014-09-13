using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using OrigoDB.Core.Utilities;
using System;

namespace OrigoDB.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for GzipHelperTest and is intended
    ///to contain all GzipHelperTest Unit Tests
    ///</summary>
    [TestFixture]
    public class GzipHelperTest
    {

        /// <summary>
        ///A test for Compress
        ///</summary>
        [Test]
        public void CompressTest()
        {
            var model = new TestModel();
            model.AddCustomer("Homer");
            model.AddCustomer("Bart");
            model.AddCustomer("Beavis");

            byte[] expected = new BinaryFormatter().ToByteArray(model);
            byte[] compressed = expected.Compress();
            Console.WriteLine("Bytes before: " + expected.Length + ", after: " + compressed.Length);
            byte[] actual = compressed.Decompress();
            Assert.IsTrue(expected.EqualsEx(actual));
        }
    }
}
