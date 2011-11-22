using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using LiveDomain.Core;
using System.Threading;
using System.Web;
using System.Net;

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
             _logger = new InMemoryLogger();
             Log.SetLogger(_logger);
             Path = System.IO.Path.Combine("c:\\temp", Guid.NewGuid().ToString());
         }

         InMemoryLogger _logger;

         //Use TestCleanup to run code after each test has run
         [TestCleanup()]
         public void MyTestCleanup() 
         {
             if (Engine != null)
             {
                 Engine.Close();
                 Thread.Sleep(TimeSpan.FromMilliseconds(100));
                 if(Directory.Exists(Path)) new DirectoryInfo(Path).Delete(true);
             }
         }
        
        #endregion


         public string Path { get; set; }
         public Engine Engine { get; set; }

         public EngineConfiguration CreateConfig()
         {
             var config = new EngineConfiguration(Path);
             config.JournalWriterPerformance = JournalWriterPerformanceMode.Asynchronous;
             config.SnapshotBehavior = SnapshotBehavior.AfterRestore;
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
            config.SetDefaultLocation<M>();
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
        public void CanExecuteCommand()
        {
            CanLoadEngine();
            int commandsExecuted = (int) this.Engine.Execute(new TestCommand());
            int numCommandsExecuted = (int)Engine.Execute(new GetNumberOfCommandsExecutedQuery());
            Assert.AreEqual(numCommandsExecuted, 1);
        }

        [TestMethod]
        public void JournalRollsOver()
        {
            CanLoadEngine();
            for (int i = 0; i < 100; i++)
            {
                Command command = new TestCommand() { Payload = new byte[100000] };
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
            Assert.AreEqual(numCommandsExecuted, 1);
        }

        [TestMethod]
        public void OnLoadIsCalledAfterRestore()
        {
            ModelRetainsStateAfterRestore();
			bool onLoadWasCalled = Engine.Execute<TestModel, bool>(m => m.OnLoadExecuted);
            Assert.IsTrue(onLoadWasCalled);
        }

        string _name;
        [TestMethod]
        public void CanCreateSnapshotWithGuidAsName()
        {
            CanLoadEngine();
            _name = Guid.NewGuid().ToString();
            Engine.CreateSnapshot(_name);
        }

        [TestMethod]
        public void CanCreateSnapshotWithEmptyName()
        {
            CanLoadEngine();
            Engine.CreateSnapshot(String.Empty);

            Engine.CreateSnapshot();
        }

        [TestMethod]
        public void LoadOrCreateCreatesWhenNotExists()
        {
            this.Engine = Engine.LoadOrCreate<TestModel>();
            Assert.IsTrue(_logger.Messages.Any(m => m.Contains("Engine Created")));
        }


        [TestMethod]
        public void LoadOrCreateCreatesInWebContext()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void LoadOrCreateLoadsInWebContext()
        {
            Assert.Inconclusive();
        }

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




        private void ExecuteCommands(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Engine.Execute(new TestCommand());
            }
        }

        private int GetCommandsExecuted()
        {
            return Engine.Execute<TestModel, int>(m => m.CommandsExecuted);
        }
    }
}
