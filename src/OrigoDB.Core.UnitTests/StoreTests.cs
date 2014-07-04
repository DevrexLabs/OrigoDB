using System.Linq;
using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Core.Journaling;
using OrigoDB.Core.Storage;
using OrigoDB.Core.Test;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class StoreTests
    {

        EngineConfiguration _config;
        private ICommandStore _commandStore;

        [SetUp]
        public void Init()
        {
            _config = EngineConfiguration.Create().ForIsolatedTest().ForImmutability();
            _commandStore = new InMemoryCommandStore(_config);
            _commandStore.Initialize();
        }

        [Test]
        public void Can_create_with_ModelCreatedEntry_in_journal()
        {

            JournalAppender.Create(0, _commandStore).AppendModelCreated(typeof(ImmutableModel));
            var entries = _commandStore.GetJournalEntries().ToArray();
            Assert.AreEqual(1, entries.Length);
            var firstEntry = entries[0] as JournalEntry<ModelCreated>;
            Assert.NotNull(firstEntry);
            Assert.AreEqual(firstEntry.Item.Type, typeof(ImmutableModel));
            Assert.AreEqual(0, firstEntry.Id);
        }

        [Test]
        public void Can_load_from_journal_with_ModelCreatedEntry()
        {
            JournalAppender.Create(0, _commandStore).AppendModelCreated(typeof(ImmutableModel));
            Model model = new ModelLoader(_config, _commandStore).LoadModel();
            Assert.IsInstanceOf<ImmutableModel>(model);
        }
    }
}