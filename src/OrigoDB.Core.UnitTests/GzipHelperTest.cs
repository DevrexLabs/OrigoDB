using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class GzipHelperTest
    {
        [Test]
        public void CompressTest()
        {
            var model = new TestModel();
            model.AddCustomer("Homer");
            model.AddCustomer("Bart");
            model.AddCustomer("Beavis");

            byte[] expected = new BinaryFormatter().ToByteArray(model);
            byte[] compressed = expected.Compress();
            Trace.WriteLine("Bytes before: " + expected.Length + ", after: " + compressed.Length);
            byte[] actual = compressed.Decompress();
            Assert.IsTrue(expected.EqualsEx(actual));
        }
    }
}
