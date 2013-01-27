using LiveDomain.Core.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LiveDomain.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for ByteCountingNullStreamTest and is intended
    ///to contain all ByteCountingNullStreamTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ByteCountingNullStreamTest
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
        public void Write_throws_when_buffer_is_null()
        {
            ByteCountingNullStream target = new ByteCountingNullStream();
            target.Write(null, 0, 1);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void Write_throws_when_buffer_offset_equals_length()
        {
            ByteCountingNullStream target = new ByteCountingNullStream();
            byte[] buffer = new byte[100];
            target.Write(buffer, 100, 1);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void Write_throws_when_buffer_offset_and_count_exceed_length()
        {
            ByteCountingNullStream target = new ByteCountingNullStream();
            byte[] buffer = new byte[100];
            target.Write(buffer, 50, 51);
        }

        [TestMethod()]
        public void Write_accepts_last_byte_of_buffer()
        {
            ByteCountingNullStream target = new ByteCountingNullStream();
            byte[] buffer = new byte[100];
            target.Write(buffer, 99, 1);
            Assert.AreEqual(1, target.Length);
        }


    }
}
