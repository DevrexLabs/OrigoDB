using System.Linq;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class PersistenceModeTests
    {
        [Test]
        public void JournalingIsDefault()
        {
            var config = EngineConfiguration.Create();
            Assert.AreEqual(config.PersistenceMode, PersistenceMode.Journaling);
        }

        [Test]
        public void SnapshotPerTransactionCreatesSnapshotsAndNoJournalEntries()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest().ForImmutability();
            config.PersistenceMode = PersistenceMode.SnapshotPerTransaction;
            var engine = Engine.Create<ImmutableModel>(config);
            engine.Execute(new AppendNumberCommand(2));
            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(12));
            engine.Dispose();

            var store = config.CreateStore();
            Assert.AreEqual(4, store.Snapshots.Count());
            Assert.AreEqual(0, store.GetJournalEntries().OfType<JournalEntry<Command>>().Count());
        }


        [Test]
        public void ManualSnaphots()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest().ForImmutability();
            config.PersistenceMode = PersistenceMode.ManualSnapshots;
            var engine = Engine.Create<ImmutableModel>(config);
            engine.Execute(new AppendNumberCommand(2));
            engine.Execute(new AppendNumberCommand(42));
            engine.CreateSnapshot();
            engine.Execute(new AppendNumberCommand(12));
            engine.Dispose();

            var store = config.CreateStore();
            Assert.AreEqual(2, store.Snapshots.Count());

            engine = Engine.Load<ImmutableModel>(config);
            var sum = engine.Execute(new NumberSumQuery());
            Assert.AreEqual(44,sum);
        }
    }
}