using System.Runtime.Serialization.Formatters.Binary;
using LiveDomain.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.Generic;

namespace LiveDomain.Core.Test
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Serializer_throws_when_passed_a_null_argument()
        {
            new Serializer(null);
        }



        [TestMethod()]
        public void SizeOf_reports_actual_size()
        {
            IFormatter formatter = new BinaryFormatter();
            Serializer target = new Serializer(formatter);
            var testModel = new TestModel();
            testModel.AddCustomer("Homer");
            testModel.AddCustomer("Bart");
            long actual = target.SizeOf(testModel);
            long expected = target.Serialize(testModel).Length;
            Console.WriteLine("SizeOf(TestModel with 2 customers) using BinaryFormatter = " + expected);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SizeOf_throws_when_passed_null_graph()
        {
            IFormatter formatter = new BinaryFormatter();
            Serializer target = new Serializer(formatter);
            TestModel testModel = null;
            long actual = target.SizeOf(testModel);
        }

    }
}
