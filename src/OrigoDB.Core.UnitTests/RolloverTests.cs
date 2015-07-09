using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class RolloverTests
    {
        [Test]
        public void CompositeIsDefault()
        {
            var config = EngineConfiguration.Create();
            var strategy = config.CreateRolloverStrategy();
            Assert.IsInstanceOf<CompositeRolloverStrategy>(strategy);
        }

        [Test]
        public void EitherMaxTriggersRollover()
        {
            var config = EngineConfiguration.Create();
            var strategy = config.CreateRolloverStrategy();
            var maxBytes = config.MaxBytesPerJournalSegment;
            var maxEntries = config.MaxEntriesPerJournalSegment;
            
            bool triggered = strategy.Rollover(maxBytes, 0);
            Assert.AreEqual(true, triggered);

            triggered = strategy.Rollover(0, maxEntries);
            Assert.AreEqual(true, triggered);

            triggered = strategy.Rollover(maxBytes - 1, maxEntries - 1);
            Assert.AreEqual(false, triggered);

            triggered = strategy.Rollover(maxBytes, maxEntries);
            Assert.IsTrue(triggered);
        }
    }
}