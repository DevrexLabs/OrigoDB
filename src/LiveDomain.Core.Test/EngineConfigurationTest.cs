using LiveDomain.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;
using System.IO;
using LiveDomain.Core.Security;

namespace LiveDomain.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for EngineConfigurationTest and is intended
    ///to contain all EngineConfigurationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EngineConfigurationTest
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
        ///A test for EngineConfiguration Constructor
        ///</summary>
        [TestMethod()]
        public void EngineConfigurationConstructorTest()
        {
        }

        /// <summary>
        ///A test for Create
        ///</summary>
        [TestMethod()]
        public void CreateTest()
        {
            EngineConfiguration expected = null; // TODO: Initialize to an appropriate value
            EngineConfiguration actual;
            actual = EngineConfiguration.Create();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateCommandJournal
        ///</summary>
        [TestMethod()]
        public void CreateCommandJournalTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            ICommandJournal expected = null; // TODO: Initialize to an appropriate value
            ICommandJournal actual;
            actual = target.CreateCommandJournal();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateFormatter
        ///</summary>
        [TestMethod()]
        public void CreateFormatterTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            IFormatter expected = null; // TODO: Initialize to an appropriate value
            IFormatter actual;
            actual = target.CreateFormatter();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateJournalWriter
        ///</summary>
        [TestMethod()]
        public void CreateJournalWriterTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            Stream stream = null; // TODO: Initialize to an appropriate value
            IJournalWriter expected = null; // TODO: Initialize to an appropriate value
            IJournalWriter actual;
            actual = target.CreateJournalWriter(stream);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateLockingStrategy
        ///</summary>
        [TestMethod()]
        public void CreateLockingStrategyTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            ISynchronizer expected = null; // TODO: Initialize to an appropriate value
            ISynchronizer actual;
            actual = target.CreateSynchronizer();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateSerializer
        ///</summary>
        [TestMethod()]
        public void CreateSerializerTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            ISerializer expected = null; // TODO: Initialize to an appropriate value
            ISerializer actual;
            actual = target.CreateSerializer();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateStorage
        ///</summary>
        [TestMethod()]
        public void CreateStorageTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            IStorage expected = null; // TODO: Initialize to an appropriate value
            IStorage actual;
            actual = target.CreateStorage();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetAuthorizer
        ///</summary>
        [TestMethod()]
        public void GetAuthorizerTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            IAuthorizer<Type> expected = null; // TODO: Initialize to an appropriate value
            IAuthorizer<Type> actual;
            actual = target.GetAuthorizer();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetDefaultDirectory
        ///</summary>
        [TestMethod()]
        public void GetDefaultDirectoryTest()
        {
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = EngineConfiguration.GetDefaultDirectory();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }





 

        /// <summary>
        ///A test for SetCommandJournalFactory
        ///</summary>
        [TestMethod()]
        public void SetCommandJournalFactoryTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            Func<ICommandJournal> factory = null; // TODO: Initialize to an appropriate value
            target.SetCommandJournalFactory(factory);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetCustomFormatterFactory
        ///</summary>
        [TestMethod()]
        public void SetCustomFormatterFactoryTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            Func<EngineConfiguration, IFormatter> factory = null; // TODO: Initialize to an appropriate value
            target.SetCustomFormatterFactory(factory);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetCustomSerializerFactory
        ///</summary>
        [TestMethod()]
        public void SetCustomSerializerFactoryTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            Func<EngineConfiguration, ISerializer> factory = null; // TODO: Initialize to an appropriate value
            target.SetCustomSerializerFactory(factory);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetCustomStorageFactory
        ///</summary>
        [TestMethod()]
        public void SetCustomStorageFactoryTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            Func<IStorage> factory = null; // TODO: Initialize to an appropriate value
            target.SetCustomStorageFactory(factory);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetLocationFromType
        ///</summary>
        public void SetLocationFromTypeTestHelper<M>()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            target.SetLocationFromType<M>();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        [TestMethod()]
        public void SetLocationFromTypeTest()
        {
            SetLocationFromTypeTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for CloneCommands
        ///</summary>
        [TestMethod()]
        public void CloneCommandsTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.CloneCommands = expected;
            actual = target.CloneCommands;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }



        /// <summary>
        ///A test for HasAlternativeSnapshotLocation
        ///</summary>
        [TestMethod()]
        public void HasAlternativeSnapshotLocationTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.HasAlternativeSnapshotLocation;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for HasLocation
        ///</summary>
        [TestMethod()]
        public void HasLocationTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.HasLocation;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for JournalSegmentSizeInBytes
        ///</summary>
        [TestMethod()]
        public void JournalSegmentSizeInBytesTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            target.JournalSegmentSizeInBytes = expected;
            actual = target.JournalSegmentSizeInBytes;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for JournalWriterPerformance
        ///</summary>
        [TestMethod()]
        public void JournalWriterPerformanceTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            JournalWriterPerformanceMode expected = new JournalWriterPerformanceMode(); // TODO: Initialize to an appropriate value
            JournalWriterPerformanceMode actual;
            target.JournalWriterPerformance = expected;
            actual = target.JournalWriterPerformance;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Location
        ///</summary>
        [TestMethod()]
        public void LocationTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Location = expected;
            actual = target.Location;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for LockTimeout
        ///</summary>
        [TestMethod()]
        public void LockTimeoutTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.LockTimeout = expected;
            actual = target.LockTimeout;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SerializationMethod
        ///</summary>
        [TestMethod()]
        public void SerializationMethodTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            SerializationMethod expected = new SerializationMethod(); // TODO: Initialize to an appropriate value
            SerializationMethod actual;
            target.SerializationMethod = expected;
            actual = target.SerializationMethod;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SnapshotBehavior
        ///</summary>
        [TestMethod()]
        public void SnapshotBehaviorTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            SnapshotBehavior expected = new SnapshotBehavior(); // TODO: Initialize to an appropriate value
            SnapshotBehavior actual;
            target.SnapshotBehavior = expected;
            actual = target.SnapshotBehavior;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SnapshotLocation
        ///</summary>
        [TestMethod()]
        public void SnapshotLocationTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.SnapshotLocation = expected;
            actual = target.SnapshotLocation;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for StorageMode
        ///</summary>
        [TestMethod()]
        public void StorageModeTest()
        {
            string targetLocation = string.Empty; // TODO: Initialize to an appropriate value
            EngineConfiguration target = new EngineConfiguration(targetLocation); // TODO: Initialize to an appropriate value
            StorageMode expected = new StorageMode(); // TODO: Initialize to an appropriate value
            StorageMode actual;
            target.StorageMode = expected;
            actual = target.StorageMode;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
