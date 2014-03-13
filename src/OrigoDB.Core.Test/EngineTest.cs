using System;
using System.Linq;
using OrigoDB.Core.Proxy;
using OrigoDB.Core.TinyIoC;
using OrigoDB.Modules.SqlStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrigoDB.Core.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class EngineTest : EngineTestBase
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
         //Use TestInitialize to run code before running each test 



        //Use TestCleanup to run code after each test has run

        #endregion

        [TestMethod]
        public void CanCreateEngine()
        {
            this.Engine = Engine.Create<TestModel>(CreateConfig());
        }

        [TestMethod]
        public void CanCreateEngineUsingDefaultLocationAndConfig()
        {

            var engine = Engine.Create<TestModel>(CreateConfig());
            engine.Close();
        }

        [TestMethod]
        public void CanLoadEngine()
        {
            var config = CreateConfig();
            var engine = Engine.Create<TestModel>(config);
            engine.Close();
            Engine.Load(config);
        }

        [TestMethod]
        public void CanExecuteCommandWithResults()
        {
            CanCreateEngine();
            int commandsExecuted = (int) this.Engine.Execute(new TestCommandWithResult());
            int numCommandsExecuted = (int)Engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.AreEqual(numCommandsExecuted, 1);
        }

        [TestMethod]
        public void CanExecuteCommand()
        {
            Engine = Engine.LoadOrCreate<TestModel>(CreateConfig());
            int commandsExecutedBefore = (int) Engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Engine.Execute(new TestCommandWithoutResult());
            int commandsExecutedAfter = (int) Engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.AreEqual(commandsExecutedAfter - commandsExecutedBefore, 1);
        }


        [TestMethod]
        public void JournalRollsOverWhenEntryCountExceedsLimit()
        {
            var config = CreateConfig();
            if (config.CreateStore() is SqlStore) return;
            config.MaxEntriesPerJournalSegment = 90;
            Engine = Engine.LoadOrCreate<TestModel>(config);
            for (int i = 0; i < 100; i++)
            {
                Command command = new TestCommandWithResult() { Payload = new byte[100000] };
                this.Engine.Execute(command);
            }
            Assert.Inconclusive();
            //Assert.IsTrue(_memoryLogWriter.Messages.Count(m => m.Contains("NewJournalSegment")) > 0);
        }

        [TestMethod]
        public void JournalRollsOverWhenSegmentSizeExceedsLimit()
        {
            
            var config = CreateConfig();

            //turn off compression
            config.PacketOptions = null;
            if (config.CreateStore() is SqlStore) return;
            config.MaxBytesPerJournalSegment = 1024 * 1024;
            Engine = Engine.LoadOrCreate<TestModel>(config);
            for (int i = 0; i < 100; i++)
            {
                Command command = new TestCommandWithResult() { Payload = new byte[100000] };
                this.Engine.Execute(command);
            }
            Assert.Inconclusive();
            //Assert.IsTrue(_memoryLogWriter.Messages.Count(m => m.Contains("NewJournalSegment")) > 0);
        }

        [TestMethod]
        public void ModelRetainsStateAfterRestore()
        {
            var config = CreateConfig();
            var engine = Engine.Create<TestModel>(config);
            engine.Execute(new TestCommandWithResult());
            engine.Close();
            engine =Engine.Load<TestModel>(config);
            int numCommandsExecuted = (int)engine.Execute(new GetNumberOfCommandsExecutedQuery());
            engine.Close();
            
            Assert.AreEqual(1, numCommandsExecuted);
        }

        [TestMethod]
        public void OnLoadIsCalledAfterRestore()
        {
            var config = CreateConfig();
            var engine = Engine.Create<TestModel>(config);
            engine.Close();
            engine = Engine.Load<TestModel>(config);
			bool onLoadWasCalled = engine.Execute<TestModel, bool>(m => m.OnLoadExecuted);
            Assert.IsTrue(onLoadWasCalled);
        }


        [TestMethod]
        public void TinyIocResolvesNamedRegistration()
        {
            var registry = new TinyIoCContainer();
            string name = Stores.FileSystem.ToString();
            registry.Register<IStore>((c,p) => new FileStore(new EngineConfiguration()), name);
            var result = registry.Resolve<IStore>(name);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateSerializerResolvesToDefault()
        {
            var config = new EngineConfiguration();
            var serializer = config.CreateSerializer();
            Assert.IsTrue(serializer is Serializer);
        }

        [TestMethod]
        public void JournalEntriesAreSequentiallyNumberedFromOne()
        {
            var config = CreateConfig();
            config.MaxEntriesPerJournalSegment = 50;
            Engine = Engine.Create(new TestModel(), config);

            ExecuteCommands(1000);

            Engine.Close();
            var storage = config.CreateStore();
            storage.Load();
            AssertJournalEntriesAreSequential(storage);
            if (storage is FileStore)
            {
                foreach (var file in (storage as FileStore).JournalFiles)
                {
                    Console.WriteLine(file);
                }
            }
        }

        [TestMethod]
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
            AssertJournalEntriesAreSequential(config.CreateStore());

        }

        private void AssertJournalEntriesAreSequential(IStore storage)
        {
            int expected = 1;
            foreach (var journalEntry in storage.GetJournalEntries())
            {
                Assert.AreEqual(expected, journalEntry.Id);
                expected++;
            }

        }

        [TestMethod]
        public void EntriesAreSequentiallyNumberedAfterReload()
        {
            var config = CreateConfig();
            config.MaxEntriesPerJournalSegment = 50;
            Engine = Engine.Create(new TestModel(), config);
            ExecuteCommands(60);
            Engine.Close();
            Engine = Engine.Load(config);
            ExecuteCommands(60);
            Engine.Close();

            //We should have 120 commands in the journal, numbered from 1 to 120
            var store = config.CreateStore();
            store.Load();
            int expected = 1;
            foreach (var journalEntry in store.GetJournalEntries())
            {
                Assert.AreEqual(expected, journalEntry.Id);
                expected++;
            }
            Assert.AreEqual(expected, 121);
            if (store is FileStore)
            {
                foreach (var file in ((FileStore) store).JournalFiles)
                {
                    Console.WriteLine(file);
                }
            }
        }

        [TestMethod]
        public void NoEmptyJournalFiles()
        {
            var config = CreateConfig();
            Engine = Engine.Create(new TestModel(), config);
            Engine.Close();
            Engine = Engine.Load(config);
            Engine.Close();
            var store = config.CreateStore() as FileStore;
            if (store != null)
            {
                store.Load();
                Assert.IsFalse(store.JournalFiles.Any());
            }
        }

        [TestMethod]
        public void NoEmptyJournalFileOnRollover()
        {
            var config = CreateConfig();
            config.MaxEntriesPerJournalSegment = 2;
            Engine = Engine.Create(new TestModel(), config);
            ExecuteCommands(2);
            //Assert.IsFalse(_memoryLogWriter.Messages.Any(m => m.Contains("NewJournalSegment")));
			ExecuteCommands(1);
			//Assert.IsTrue(_memoryLogWriter.Messages.Any(m => m.Contains("NewJournalSegment")));
            Engine.Close();
			
            var store = config.CreateStore() as FileStore;
            if (store != null)
            {
                store.Load();
                Assert.AreEqual(2, store.JournalFiles.Count());
                foreach (var file in store.JournalFiles)
                {
                    Console.WriteLine(file);
                }
            }
            else Assert.Inconclusive("test isn't relevant under the current configuration, requires FileStore storage");
            Assert.Inconclusive();            
        }

        private void ExecuteCommands(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Engine.Execute(new TestCommandWithResult());
            }
        }

        private int GetCommandsExecuted()
        {
            return Engine.Execute<TestModel, int>(m => m.CommandsExecuted);
        }
    }
}
