using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using LiveDomain.Core;

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
             Path = System.IO.Path.Combine(TestContext.TestRunResultsDirectory, Guid.NewGuid().ToString());
         }
        

         //Use TestCleanup to run code after each test has run
         [TestCleanup()]
         public void MyTestCleanup() 
         {
             if(Engine != null) Engine.Close();
         }
        
        #endregion


         public string Path { get; set; }
         public Engine Engine { get; set; }

        [TestMethod]
        public void CanCreateEngine()
        {
            Engine.Create(new TestModel(), new EngineSettings(Path));
        }

        [TestMethod]
        public void CanLoadEngine()
        {
            CanCreateEngine();
            this.Engine = Engine.Load(Path);
            
        }

        [TestMethod]
        public void CommandPermutesModel()
        {
            CanLoadEngine();
            int commandsExecuted = (int) this.Engine.Execute(new TestCommand());

        }

        [TestMethod]
        public void ModelRetainsStateAfterRestore()
        {
            CommandPermutesModel();
            Engine.Close();
            Engine = Engine.Load(Path);
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
        public void CanCreateSnapshot()
        {
            CanLoadEngine();
            _name = new Guid().ToString();
            Engine.CreateSnapshot(_name);
        }

        [TestMethod]
        public void CanRestoreSnapshot()
        {
            CanCreateSnapshot();
            int commandsBefore = GetCommandsExecuted();
            ExecuteCommands(3);
            Assert.AreEqual(commandsBefore + 3, GetCommandsExecuted());
            Engine.RevertToSnapshot(_name);
            Assert.AreEqual(commandsBefore, GetCommandsExecuted());
        }

        [TestMethod]
        public void Journal_is_cleared_on_snapshot_revert()
        {
            CanRestoreSnapshot();
            Assert.IsTrue(Engine.CommandJournal.Count() == 0);

        }

        [TestMethod]
        public void Snapshot_info_gets_returned()
        {
            CanCreateSnapshot();
            Assert.AreEqual(1, Engine.GetSnapshots().Count());
        }

        /// <summary>
        ///A test for RevertToImage
        ///</summary>
        [TestMethod()]
        public void RevertToImageTest()
        {
            CanLoadEngine();
            int commandsExecuted = GetCommandsExecuted();
            Assert.IsTrue(GetCommandsExecuted() == 0);
            ExecuteCommands(5);
            Assert.IsTrue(GetCommandsExecuted() == 5);
            Engine.WriteBaseImage();
            Assert.IsTrue(GetCommandsExecuted() == 5);
            ExecuteCommands(4);
            Assert.IsTrue(GetCommandsExecuted() == 9);
            Engine.RevertToImage();
            Assert.IsTrue(GetCommandsExecuted() == 5);
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

        [TestMethod]
        public void JournalIsClearedOnWriteImage()
        {
            CanLoadEngine();
            ExecuteCommands(new Random().Next(10) + 1);
            Engine.WriteBaseImage();
            Assert.IsTrue(Engine.CommandJournal.Count() == 0);
        }

        [TestMethod]
        public void Can_compile_canaresiska()
        {
            Engine ಠ_ಠ = this.Engine;
        }

    }
}
