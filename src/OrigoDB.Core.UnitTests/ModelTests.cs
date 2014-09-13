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

    }
}