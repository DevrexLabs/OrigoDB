using System.Linq;
using NUnit.Framework;
using OrigoDB.Core.Journaling;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class StoreTests
    {

        EngineConfiguration _config;
        private IStore _store;


        [SetUp]
        public void Init()
        {
            _config = EngineConfiguration.Create().ForIsolatedTest().ForImmutability();
            _store = new InMemoryStore(_config);
            _store.Init();
            
        }

        [Test]
        public void Create_from_type_creates_journal_entry()
        {

            _store.Create(typeof(ImmutableModel));
            var entries = _store.GetJournalEntries().ToArray();
            Assert.AreEqual(1, entries.Length);
            var firstEntry = entries[0] as JournalEntry<ModelCreated>;
            Assert.NotNull(firstEntry);
            Assert.AreEqual(firstEntry.Item.Type, typeof(ImmutableModel));
            Assert.IsFalse(_store.Snapshots.Any());
        }

        [Test]
        public void Can_load_from_journal_only()
        {
            _store.Create(typeof(ImmutableModel));
            Model model = _store.LoadModel();
            Assert.IsInstanceOf<ImmutableModel>(model);
        }

        [Test]
        public void Can_load_from_type()
        {
            var model = (ImmutableModel) _store.LoadModel(typeof (ImmutableModel));
            var engine = new Engine<ImmutableModel>(model, _store, _config);
            engine.Execute(new AppendNumberCommand(53));
            engine.Execute(new AppendNumberCommand(42));
            engine.Close();
            _store = new InMemoryStore(_config);
            _store.Init();
            model = (ImmutableModel) _store.LoadModel(typeof (ImmutableModel));
            
            //make sure state is valid after restore
            Assert.AreEqual(53 + 42, model.Numbers().Sum());
            

            var ids = _store.GetJournalEntries().Select(je => (int) je.Id).ToArray();
            Assert.AreEqual(Enumerable.Range(1,2),ids);

        }

    }
}