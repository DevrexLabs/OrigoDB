using System;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using OrigoDB.Core.Journaling;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class StoreTests
    {
        [Test]
        public void Create_from_type_creates_journal_entry()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest();
            var store = new InMemoryStore(config);
            store.Init();

            store.Create(typeof(ImmutableModel));
            var entries = store.GetJournalEntries().ToArray();
            Assert.AreEqual(1, entries.Length);
            var firstEntry = entries[0] as JournalEntry<ModelCreated>;
            Assert.NotNull(firstEntry);
            Assert.AreEqual(firstEntry.Item.Type, typeof(ImmutableModel));
            Assert.IsFalse(store.Snapshots.Any());
        }

        [Test]
        public void Can_load_from_journal_only()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest();
            var store = new InMemoryStore(config);
            store.Init();

            store.Create(typeof(ImmutableModel));
            Model model = store.LoadModel();
            Assert.IsInstanceOf<ImmutableModel>(model);
        }

        [Test]
        public void Can_load_from_type()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest().ForImmutability();
            var store = new InMemoryStore(config);
            store.Init();

            var model = (ImmutableModel) store.LoadModel(typeof (ImmutableModel));
            var engine = new Engine<ImmutableModel>(model, store, config);
            engine.Execute(new AppendNumberCommand(53));
            engine.Execute(new AppendNumberCommand(42));
            engine.Dispose();
            store = new InMemoryStore(config);
            store.Init();
            model = (ImmutableModel) store.LoadModel(typeof (ImmutableModel));
            Assert.AreEqual(53 + 42, model.Numbers().Sum());

            ulong id = 1;
            var ids = store.GetJournalEntries().Select(je => (int) je.Id).ToArray();
            Assert.AreEqual(Enumerable.Range(1,2),ids);

        }

    }
}