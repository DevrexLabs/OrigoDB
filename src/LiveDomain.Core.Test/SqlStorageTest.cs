using LiveDomain.Modules.SqlStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using LiveDomain.Core;
using System.Data;
using System.Data.Common;
using System.IO;

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
    }
}
