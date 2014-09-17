using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class ModelTests
    {
        [Test]
        public void EventDispatcherReturnsSameObjectOnSubsequentCalls()
        {
            var target = new TestModel();
            Assert.AreSame(target.EventDispatcher, target.EventDispatcher);
        }

        [Test]
        public void EventDispatcherIsFilteringEventDispatcher()
        {
            var target = new TestModel();
            Assert.IsInstanceOf<FilteringEventDispatcher>(target.EventDispatcher);
        }

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