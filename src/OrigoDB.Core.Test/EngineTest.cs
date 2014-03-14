using System;
using System.Linq;
using OrigoDB.Core.Proxy;
using OrigoDB.Core.TinyIoC;
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
            Engine.Create<TestModel>(CreateConfig());
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
            var engine = Engine.For<TestModel>(CreateConfig());
            engine.Execute(new TestCommandWithResult());
            int numCommandsExecuted = engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.AreEqual(1, numCommandsExecuted);
        }

        [TestMethod]
        public void CanExecuteCommand()
        {
            var engine = Engine.LoadOrCreate<TestModel>(CreateConfig());
            int commandsExecutedBefore = engine.Execute(new GetNumberOfCommandsExecutedQuery());
            engine.Execute(new TestCommandWithoutResult());
            int commandsExecutedAfter = engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.AreEqual(1, commandsExecutedAfter - commandsExecutedBefore);
        }


        [TestMethod]
        public void JournalRollsOverWhenEntryCountExceedsLimit()
        {
            var config = CreateConfig();
            config.MaxEntriesPerJournalSegment = 90;
            var engine = Engine.LoadOrCreate<TestModel>(config);
            for (int i = 0; i < 100; i++)
            {
                Command command = new TestCommandWithResult() { Payload = new byte[100000] };
                engine.Execute(command);
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
            config.MaxBytesPerJournalSegment = 1024 * 1024;
            var engine = Engine.LoadOrCreate<TestModel>(config);
            for (int i = 0; i < 100; i++)
            {
                Command command = new TestCommandWithResult() { Payload = new byte[100000] };
                engine.Execute(command);
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
            int numCommandsExecuted = engine.Execute(new GetNumberOfCommandsExecutedQuery());
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
            var engine = Engine.Create(new TestModel(), config);

            ExecuteCommands(engine,1000);

            engine.Close();
            var storage = config.CreateStore();
            storage.Load();
            AssertJournalEntriesAreSequential(storage);
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
            Console.WriteLine("JournalEntry Ids:");
            foreach (var journalEntry in storage.GetJournalEntries())
            {
                Console.WriteLine(journalEntry.Id);
                Assert.AreEqual(expected, journalEntry.Id);
                expected++;
            }

        }

        [TestMethod]
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

            var store = config.CreateStore();

            AssertJournalEntriesAreSequential(store);
            Assert.AreEqual(120, store.LastEntryId);
            Assert.AreEqual(120, store.GetJournalEntries().Count());
        }

        [TestMethod]
        public void NoEmptyJournalFiles()
        {
            var config = CreateConfig();
            var engine = Engine.Create(new TestModel(), config);
            engine.Close();
            engine = Engine.Load<TestModel>(config);
            engine.Close();
            var store = config.CreateStore() as FileStore;
            if (store != null)
            {
                store.Load();
                Assert.IsFalse(store.JournalFiles.Any());
            }
            Assert.Inconclusive();
        }

        [TestMethod]
        public void NoEmptyJournalFileOnRollover()
        {
            var config = CreateConfig();
            config.MaxEntriesPerJournalSegment = 2;
            var engine = Engine.Create(new TestModel(), config);
            ExecuteCommands(engine,2);
            //Assert.IsFalse(_memoryLogWriter.Messages.Any(m => m.Contains("NewJournalSegment")));
			ExecuteCommands(engine,1);
			//Assert.IsTrue(_memoryLogWriter.Messages.Any(m => m.Contains("NewJournalSegment")));
            engine.Close();
			
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

        private void ExecuteCommands(Engine engine, int count)
        {
            for (int i = 0; i < count; i++)
            {
                engine.Execute(new TestCommandWithResult());
            }
        }
    }
}
