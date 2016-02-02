using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class EngineTest : EngineTestBase
    {

        [Test]
        public void CanCreateEngine()
        {
            Engine.Create<TestModel>(CreateConfig());
        }

        [Test]
        public void CanCreateEngineUsingDefaultLocationAndConfig()
        {

            var engine = Engine.Create<TestModel>(CreateConfig());
            engine.Close();
        }

        [Test]
        public void CanLoadEngine()
        {
            var config = CreateConfig();
            var engine = Engine.Create<TestModel>(config);
            engine.Close();
            Engine.Load(config);
        }

        [Test]
        public void CanExecuteCommandWithResults()
        {
            var engine = Engine.For<TestModel>(CreateConfig());
            engine.Execute(new TestCommandWithResult());
            int numCommandsExecuted = engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.AreEqual(1, numCommandsExecuted);
        }

        [Test]
        public void CanGetModelReference()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            var engine = Engine.Create<TestModel>(config);
            engine.Execute(new TestCommandWithoutResult());
            var model = engine.GetModel();
            Assert.IsInstanceOf(typeof(TestModel), model);
        }

        [Test]
        public void CanExecuteLambdaQuery()
        {
            var engine = Engine.LoadOrCreate<TestModel>(CreateConfig());
            engine.Execute(new TestCommandWithoutResult());
            var commandsExectuted = engine.Execute(db => db.CommandsExecuted);
            Assert.AreEqual(1, commandsExectuted);
            
        }

        [Test]
        public void CanExecuteCommand()
        {
            var engine = Engine.LoadOrCreate<TestModel>(CreateConfig());
            int commandsExecutedBefore = engine.Execute(new GetNumberOfCommandsExecutedQuery());
            engine.Execute(new TestCommandWithoutResult());
            int commandsExecutedAfter = engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.AreEqual(1, commandsExecutedAfter - commandsExecutedBefore);
        }

        [Test]
        public void ModelRetainsStateAfterRestore()
        {
            var config = CreateConfig();
            var engine = Engine.Create<TestModel>(config);
            engine.Execute(new TestCommandWithResult());
            engine.Close();
            engine =Engine.Load<TestModel>(config);
            int numCommandsExecuted = engine.Execute(new GetNumberOfCommandsExecutedQuery());
            engine.Close();           
            Assert.AreEqual(1, numCommandsExecuted);
        }

        [Test]
        public void OnLoadIsCalledAfterRestore()
        {
            var config = CreateConfig();
            var engine = Engine.Create<TestModel>(config);
            engine.Close();
            engine = Engine.Load<TestModel>(config);
			bool onLoadWasCalled = engine.Execute<TestModel,bool>(m => m.OnLoadExecuted);
            Assert.IsTrue(onLoadWasCalled);
        }

        [Test]
        public void JournalEntriesAreSequentiallyNumberedFromOne()
        {
            var config = CreateConfig();
            config.MaxEntriesPerJournalSegment = 50;
            var engine = Engine.Create(new TestModel(), config);

            ExecuteCommands(engine,1000);

            engine.Close();
            var store = config.CreateCommandStore();
            AssertJournalEntriesAreSequential(store);
        }

        [Test]
        public void JournalEntryIdSequenceIsContinuedAfterRestore()
        {
            var config = CreateConfig();
            var engine = Engine.LoadOrCreate<TestModel>(config);
            TestModel db = engine.GetProxy();
            db.AddCustomer("Homer");
            engine.Close();
            engine = Engine.LoadOrCreate<TestModel>(config);
            db = engine.GetProxy();
            db.AddCustomer("Bart");
            engine.Close();
            AssertJournalEntriesAreSequential(config.CreateCommandStore());

        }

        private void AssertJournalEntriesAreSequential(ICommandStore storage)
        {
            ulong expected = 1;
            foreach (var journalEntry in storage.CommandEntries())
            {
                Assert.AreEqual(expected, journalEntry.Id);
                expected++;
            }

        }

        [Test]
        public void EntriesAreSequentiallyNumberedAfterReload()
        {
            var config = CreateConfig();
            config.MaxEntriesPerJournalSegment = 50;
            var engine = Engine.Create<TestModel>(config);
            ExecuteCommands(engine,60);
            engine.Close();
            engine = Engine.Load<TestModel>(config);
            ExecuteCommands(engine,60);
            engine.Close();

            var store = config.CreateCommandStore();
            AssertJournalEntriesAreSequential(store);
        }

        private void ExecuteCommands(Engine engine, int count)
        {
            for (int i = 0; i < count; i++)
            {
                engine.Execute(new TestCommandWithResult());
            }
        }
    }
}
