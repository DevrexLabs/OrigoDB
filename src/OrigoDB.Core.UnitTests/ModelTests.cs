using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class ModelTests
    {
        [Test]
        public void RevisionIsIncrementedWithEachCommand()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            var target = Db.For<TestModel>(config);
            Assert.AreEqual(0, target.Revision);
            target.AddCustomer("Homer");
            Assert.AreEqual(1, target.Revision);
        }
    }
}