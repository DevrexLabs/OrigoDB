using System;
using System.Diagnostics;
using System.Linq;
using LiveDomain.Modules.SqlStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;
using LiveDomain.Core.Logging;
using TinyIoC;

namespace LiveDomain.Core.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class EngineTest
    {
        public EngineTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
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
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
         //Use TestInitialize to run code before running each test 
         [TestInitialize]
         public void MyTestInitialize() 
         {
             Path = Guid.NewGuid().ToString();
             _logger.Clear();
         }

        public void WriteLog()
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine(" LOG");
            Console.WriteLine("------------------------------");
            foreach (var message in _logger.Messages)
            {
                Console.WriteLine(message);
            }
        }

        static EngineTest()
        {
            Log.SetLogFactory(new SingletonLogFactory(_logger));
        }

         static InMemoryLogger _logger = new InMemoryLogger();

         //Use TestCleanup to run code after each test has run
         [TestCleanup()]
         public void MyTestCleanup() 
         {
             if (Engine != null)
             {
                 Engine.Close();
                 Thread.Sleep(50);
                 if (Directory.Exists(Path)) new DirectoryInfo(Path).Delete(true);
             }
             Console.WriteLine("Path:" + Path);
             WriteLog();
         }
        
        #endregion

        public Engine Engine { get; set; }
        public String Path { get; set; }
        
        /// <summary>
        /// modify this method to switch between sql and file store tests
        /// </summary>
        /// <returns></returns>
        public EngineConfiguration CreateConfig()
        {
            //return CreateSqlConfig();
            return CreateFileConfig();
        }

        private EngineConfiguration CreateFileConfig()
        {
            var config = new EngineConfiguration();

            //Connection string name in app.config file
            config.Location = Path;
            config.SnapshotBehavior = SnapshotBehavior.None;
            config.Synchronization = SynchronizationMode.ReadWrite;
            return config;
            
        }

        private SqlEngineConfiguration CreateSqlConfig()
        {
            var config = new SqlEngineConfiguration();
            
            //Connection string name in app.config file
            config.Location = "livedbstorage";
            config.SnapshotLocation = Path;

            //new table for every test. Cleanup your test database later
            config.JournalTableName = Path;

            config.SnapshotBehavior = SnapshotBehavior.None;
            config.Synchronization = SynchronizationMode.ReadWrite;
            return config;
        }

        [TestMethod]
        public void CanCreateEngine()
        {
            this.Engine = Engine.Create(new TestModel(), CreateConfig());
        }

        [TestMethod]
        public void CanCreateEngineUsingDefaultLocationAndConfig()
        {
            DeleteFromDefaultLocation<TestModel>();
            var engine = Engine.Create<TestModel>();
            engine.Close();
        }

        [TestMethod]
        public void CanCreateEngineUsingDefaultLocationAndCustomConfig()
        {
            //Test will fail if storage already exists
            DeleteFromDefaultLocation<TestModel>();
            var config = new EngineConfiguration();
            config.CloneCommands = false;
            var engine = Engine.Create<TestModel>(config);
            engine.Close();
        }

        private void DeleteFromDefaultLocation<M>() where M : Model
        {
            var config = new EngineConfiguration();
            config.SetLocationFromType<M>();
            var dirInfo = new DirectoryInfo(config.Location);
            if (dirInfo.Exists)
            {
                dirInfo.Delete(recursive: true);
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }
        }


        [TestMethod]
        public void CanLoadGeneric()
        {
            CanCreateEngine();
            Engine.Close();
            this.Engine = Engine.Load<TestModel>(CreateConfig());
        }

        [TestMethod]
        public void CanLoadEngine()
        {
            CanCreateEngine();
            Engine.Close();
            this.Engine = Engine.Load(CreateConfig());
            
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
            Assert.IsTrue(_logger.Messages.Count(m => m.Contains("NewJournalSegment")) > 0);
        }

        [TestMethod]
        public void JournalRollsOverWhenSegmentSizeExceedsLimit()
        {
            var config = CreateConfig();
            if (config.CreateStore() is SqlStore) return;
            config.MaxBytesPerJournalSegment = 1024 * 1024;
            Engine = Engine.LoadOrCreate<TestModel>(config);
            for (int i = 0; i < 100; i++)
            {
                Command command = new TestCommandWithResult() { Payload = new byte[100000] };
                this.Engine.Execute(command);
            }
            Assert.IsTrue(_logger.Messages.Count(m => m.Contains("NewJournalSegment")) > 0);
        }

        [TestMethod]
        public void ModelRetainsStateAfterRestore()
        {
            CanExecuteCommand();
            Engine.Close();
            Engine = Engine.Load(CreateConfig());
            int numCommandsExecuted = (int)Engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.IsTrue(numCommandsExecuted > 0);
        }

        [TestMethod]
        public void OnLoadIsCalledAfterRestore()
        {
            ModelRetainsStateAfterRestore();
			bool onLoadWasCalled = Engine.Execute<TestModel, bool>(m => m.OnLoadExecuted);
            Assert.IsTrue(onLoadWasCalled);
        }



        [TestMethod]
        public void LoadOrCreateCreatesWhenNotExists()
        {
            DeleteFromDefaultLocation<TestModel>();
            this.Engine = Engine.LoadOrCreate<TestModel>();
            Assert.IsTrue(_logger.Messages.Any(m => m.Contains("Engine Created")));
        }


        //[TestMethod]
        //public void LoadOrCreateCreatesInWebContext()
        //{
        //    Assert.Inconclusive();
        //}

        //[TestMethod]
        //public void LoadOrCreateLoadsInWebContext()
        //{
        //    Assert.Inconclusive();
        //}

        [TestMethod]
        public void LoadOrCreateLoadsWhenExists()
        {
            var engine = Engine.LoadOrCreate<TestModel>();
            engine.Close();
            this.Engine = Engine.LoadOrCreate<TestModel>();
            
            Assert.IsTrue(_logger.Messages.Any(m => m.Contains("Engine Loaded")));
        }

        [TestMethod]
        public void SnapshotTakenOnLoad()
        {
            DeleteFromDefaultLocation<TestModel>();
            EngineConfiguration config = new EngineConfiguration();
            config.SnapshotBehavior = SnapshotBehavior.AfterRestore;
            var engine = Engine.Create<TestModel>(config);
            engine.Close();
            Assert.IsTrue(_logger.Messages.Any(m => m.Contains("BeginSnapshot")));
            Assert.IsTrue(_logger.Messages.Any(m => m.Contains("EndSnapshot")));
        }

        [TestMethod]
        public void SnapshotTakenOnShutdown()
        {
            DeleteFromDefaultLocation<TestModel>();
            EngineConfiguration config = new EngineConfiguration();
            config.SnapshotBehavior = SnapshotBehavior.OnShutdown;
            var engine = Engine.Create<TestModel>(config);
            engine.Close();
            Assert.IsTrue(_logger.Messages.Any(m => m.Contains("BeginSnapshot")));
            Assert.IsTrue(_logger.Messages.Any(m => m.Contains("EndSnapshot")));
        }

        [TestMethod]
        public void TinyIocResolvesNamedRegistration()
        {
            var registry = new TinyIoCContainer();
            string name = StoreType.FileSystem.ToString();
            registry.Register<IStore>((c,p) => new FileStore(new EngineConfiguration()), name);
            var result = registry.Resolve<IStore>(name);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void InjectedLogFactoryIsResolved()
        {
            Log.SetLogFactory(new SingletonLogFactory(new InMemoryLogger()));
            var logger = Log.GetLogFactory().GetLogForCallingType();
            Assert.IsNotNull(logger);
            Assert.IsTrue(logger is InMemoryLogger);
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
            int expected = 1;
            foreach (var journalEntry in storage.GetJournalEntries())
            {
                Assert.AreEqual(expected, journalEntry.Id);
                expected++;
            }
            if (storage is FileStore)
            {
                foreach (var file in (storage as FileStore).JournalFiles)
                {
                    Console.WriteLine(file);
                }
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
            ExecuteCommands(3);
            Engine.Close();
            var store = config.CreateStore() as FileStore;

            if (store != null)
            {
                store.Load();
                Assert.IsTrue(store.JournalFiles.Count() == 1);
                foreach (var file in ((FileStore)store).JournalFiles)
                {
                    Console.WriteLine(file);
                }
            }
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
