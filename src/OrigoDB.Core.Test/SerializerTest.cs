using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;

namespace OrigoDB.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for SerializerTest and is intended
    ///to contain all SerializerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SerializerTest
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
        public void SizeOf_reports_actual_size()
        {
            IFormatter target = new BinaryFormatter();
            var testModel = new TestModel();
            testModel.AddCustomer("Homer");
            testModel.AddCustomer("Bart");
            long actual = target.SizeOf(testModel);
            long expected = target.ToByteArray(testModel).Length;
            Console.WriteLine("SizeOf(TestModel with 2 customers) using BinaryFormatter = " + expected);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SizeOf_throws_when_passed_null_graph()
        {
            var target = new BinaryFormatter();
            target.SizeOf(null);
        }

    }
}
