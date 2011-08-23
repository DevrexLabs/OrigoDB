using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
            //
            // TODO: Add constructor logic here
            //
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
         [TestInitialize()]
         public void MyTestInitialize() 
         {
             Path = System.IO.Path.Combine(TestContext.TestRunResultsDirectory, Guid.NewGuid().ToString());
         }
        

         //Use TestCleanup to run code after each test has run
         [TestCleanup()]
         public void MyTestCleanup() 
         {
             //Directory.Delete(Path);
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

        [TestMethod]
        public void CanExecuteGenericCommand()
        {
        }
    }
}
