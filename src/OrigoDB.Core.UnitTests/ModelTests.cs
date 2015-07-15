using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class ModelTests
    {
        [Test]
        public void RevisionIsIncrementedWithEachCommand()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest();
            var target = Db.For<TestModel>(config);
            Assert.AreEqual(0, target.Revision);
            target.AddCustomer("Homer");
            Assert.AreEqual(1, target.Revision);
        }
    }
}