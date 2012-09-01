using LiveDomain.Modules.SqlStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using LiveDomain.Core;
using System.Data;
using System.Data.Common;

namespace LiveDomain.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for SqlStorageTest and is intended
    ///to contain all SqlStorageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SqlStorageTest
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



        /// <summary>
        ///A test for VerifyCanCreate
        ///</summary>
        [TestMethod()]
        public void VerifyCanCreateTest()
        {
            EngineConfiguration config = new EngineConfiguration();
            SqlStore target = new SqlStore(config);
            target.VerifyCanCreate();
        }


        /// <summary>
        ///A test for SqlStorage Constructor
        ///</summary>
        [TestMethod()]
        public void SqlStorageConstructorTest()
        {
            EngineConfiguration config = new EngineConfiguration();
            config.Location = "livedbstorage";
            SqlStore target = new SqlStore(config);
        }

        /// <summary>
        ///A test for SqlStorage Constructor
        ///</summary>
        [TestMethod()]
        public void CanLoadSqlProviderFactory()
        {
            var dataTable = DbProviderFactories.GetFactoryClasses();
            foreach (DataColumn column in dataTable.Columns)
            {
                Console.WriteLine(column.ColumnName);
            }
            Console.WriteLine("------------------------------------------------------");
            foreach (DataRow row in dataTable.Rows)
            {

                foreach (object item in row.ItemArray)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("---------------------------------------------------");
            }

            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            Assert.IsNotNull(factory);
        }

        [TestMethod]
        public void CanWriteSnapshot()
        {
            TestModel model = new TestModel();
            var config = new EngineConfiguration();
            config.SnapshotLocation = "c:\\livedb\\sqlstorage";
            var storage = new SqlStore(config);
            storage.WriteSnapshot(model, "initial");
        }
        
        [TestMethod]
        public void CanAppendCommand()
        {
            ICommandJournal cj;
            //cj.
        }
    }
}
