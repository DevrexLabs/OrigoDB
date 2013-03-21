using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OrigoDB.Core.Logging;
using System.Reflection;
using OrigoDB.Modules.Blackbox;

namespace Modules.Blackbox.Test
{
    
    
    /// <summary>
    ///This is a test class for BlackBoxLogFactoryTest and is intended
    ///to contain all BlackBoxLogFactoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BlackBoxLogFactoryTest
    {


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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        public void Can_create_blackboxlogfactory()
        {
            new BlackBoxLogFactory();
        }

        [TestMethod()]
        public void Can_create_blackboxlogger()
        {
            new BlackBoxLogFactory().GetLogForCallingType();
        }

        [TestMethod()]
        public void Can_log_message()
        {
            new BlackBoxLogFactory().GetLogForCallingType().Info("Hello, world!");
        }
    }
}
