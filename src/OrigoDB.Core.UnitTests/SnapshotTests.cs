using System;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class SnapshotTests
    {
        [Test]
        public void Journal_file_rolls_over_after_snapshot()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            var engine = Engine.Create<TestModel>(config);
            var store = (InMemoryCommandStore)config.CreateCommandStore();

            engine.Execute(new TestCommandWithResult());
            int segmentsBefore = store.JournalSegments;
            
            engine.CreateSnapshot();
            Assert.AreEqual(segmentsBefore + 1, store.JournalSegments);
        }

        [Test]
        public void Journal_is_truncated_after_snapshot()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            config.TruncateJournalOnSnapshot = true;
            var engine = Engine.Create<TestModel>(config);
            var store = config.CreateCommandStore();

            engine.Execute(new TestCommandWithResult());

            engine.CreateSnapshot();
            ulong revision = engine.GetModel().Revision;
            engine.Execute(new TestCommandWithResult());
            Assert.AreEqual(1, store.CommandEntries().Count(), "Expected single command in store after truncate");
            Assert.AreEqual(revision + 1, store.CommandEntries().First().Id);
        }

        [Test]
        public void Snapshots_are_numbered_correctly()
        {
            var config = new EngineConfiguration().ForImmutability().ForIsolatedTest();
            var engine = Engine.Create<ImmutableModel>(config);

            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.CreateSnapshot();

            var store = config.CreateSnapshotStore();
            Assert.AreEqual(4, store.Snapshots.First().Revision);
            Assert.AreEqual(1, store.Snapshots.Count());
        }

        [Test]
        public void Entry_id_is_extracted_from_snapshot_filename()
        {
            var dt = DateTime.Now;
            Snapshot ss = FileSnapshot.FromFileInfo("000467000.snapshot", dt);
            Assert.AreEqual(dt,ss.Created);
            Assert.AreEqual(467000, ss.Revision);
        }
    }
}