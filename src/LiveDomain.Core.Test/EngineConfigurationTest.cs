using LiveDomain.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;
using System.IO;
using LiveDomain.Core.Security;
using System.Runtime.Serialization.Formatters.Binary;

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



        [TestMethod()]
        public void ObjectFormattingDefaultsToBinary()
        {
            var config = new EngineConfiguration();
            Assert.AreEqual(ObjectFormatting.NetBinaryFormatter, config.ObjectFormatting);
        }

        [TestMethod()]
        public void InjectingFormatterSetsModeToCustom()
        {
            var config = new EngineConfiguration();
            config.SetFormatterFactory((c) => null);
            Assert.AreEqual(ObjectFormatting.Custom, config.ObjectFormatting);
        }

        [TestMethod()]
        public void InjectedFormatterIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new BinaryFormatter();
            config.SetFormatterFactory((c) => expected);
            var actual = config.CreateFormatter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        public void InjectingStorageSetsModeToCustom()
        {
            var config = new EngineConfiguration();
            config.SetStorageFactory((c) => null);
            Assert.AreEqual(StorageType.Custom, config.StorageType);
        }

        [TestMethod()]
        public void FileStorageIsDefault()
        {
            var config = new EngineConfiguration();
            var storage = config.CreateStorage();
            Assert.IsTrue(storage is FileStorage);
        }

        [TestMethod()]
        public void InjectedStorageIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new NullStorage();
            config.SetStorageFactory((c) => expected);
            var actual = config.CreateStorage();
            Assert.AreEqual(expected,actual);
        }

        [TestMethod()]
        public void SynchronizerDefaultsToReadWrite()
        {
            var config = new EngineConfiguration();
            var synchronizer = config.CreateSynchronizer();
            Assert.IsTrue(synchronizer is ReadWriteSynchronizer);
        }

        [TestMethod()]
        public void InjectingSynchronizerSetsModeToCustom()
        {
            var config = new EngineConfiguration();
            config.SetSynchronizerFactory((c) => null);
            Assert.AreEqual(SynchronizationMode.Custom, config.Synchronization);
        }

        [TestMethod()]
        public void InjectedSynchronizerIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new NullSynchronizer();
            config.SetSynchronizerFactory((c) => expected);
            var actual = config.CreateSynchronizer();
            Assert.AreEqual(expected,actual);
        }

        [TestMethod()]
        public void InjectedSerializerIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new Serializer(new BinaryFormatter());
            config.SetSerializerFactory((c) => expected);
            var actual = config.CreateSerializer();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void InjectedAuthorizerIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new TypeBasedPermissionSet();
            config.SetAuthorizerFactory((c) => expected);
            var actual = config.CreateAuthorizer();
            Assert.AreEqual(expected,actual);
        }


        [TestMethod()]
        public void AsyncJournalingYieldsAsyncWriter()
        {
            var config = new EngineConfiguration();
            config.JournalWriterMode = JournalWriterMode.Asynchronous;
            var writer = config.CreateJournalWriter(new MemoryStream());
            Assert.IsTrue(writer is AsynchronousJournalWriter);
        }

        [TestMethod()]
        public void SyncJournalingYieldsSyncWriter()
        {
            var config = new EngineConfiguration();
            config.JournalWriterMode = JournalWriterMode.Synchronous;
            var writer = config.CreateJournalWriter(new MemoryStream());
            Assert.IsTrue(writer is SynchronousJournalWriter);
        }

        [TestMethod()]
        public void SyncJournalingIsDefault()
        {
            var config = new EngineConfiguration();
            Assert.AreEqual(JournalWriterMode.Synchronous, config.JournalWriterMode);
        }


        /*
JournalWriter
          */

    }
}
