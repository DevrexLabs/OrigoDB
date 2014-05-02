using OrigoDB.Core.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using OrigoDB.Core.Security;
using System.Runtime.Serialization.Formatters.Binary;

namespace OrigoDB.Core.Test
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


        [TestMethod()]
        public void InjectedFormatterIsResolved()
        {
            var config = new EngineConfiguration();
            config.PacketOptions = null;
            var expected = new BinaryFormatter();
            config.SetFormatterFactory((c,f) => expected);
            var actual = config.CreateFormatter(FormatterUsage.Default);
            Assert.AreSame(expected, actual);
        }

        [TestMethod()]
        public void PacketingFormatterIsReturnedWhenPaketOptionsArePresent()
        {
            var config = new EngineConfiguration();
            config.PacketOptions = PacketOptions.Checksum;
            config.SetFormatterFactory((c,f) => new BinaryFormatter());
            var actual = config.CreateFormatter(FormatterUsage.Journal);
            Assert.IsInstanceOfType(actual, typeof(PacketingFormatter));
        }

        [TestMethod()]
        public void InjectingStorageSetsModeToCustom()
        {
            var config = new EngineConfiguration();
            config.SetStoreFactory((c) => null);
            Assert.AreEqual(Stores.Custom, config.StoreType);
        }

        [TestMethod()]
        public void FileStorageIsDefault()
        {
            var config = new EngineConfiguration().WithRandomLocation();
            var storage = config.CreateStore();
            Assert.IsTrue(storage is FileStore);
            Directory.Delete(config.Location.OfJournal, true);
        }

        [TestMethod()]
        public void InjectedStorageIsResolved()
        {

            var config = new EngineConfiguration()
                .WithRandomLocation();
            var expected = new FileStore(config);
            config.SetStoreFactory((c) => expected);
            var actual = config.CreateStore();
            Assert.AreSame(expected, actual);
            Directory.Delete(config.Location.OfJournal, true);
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
            Assert.AreSame(expected,actual);
        }

        [TestMethod()]
        public void InjectedAuthorizerIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new TypeBasedPermissionSet();
            config.SetAuthorizerFactory((c) => expected);
            var actual = config.CreateAuthorizer();
            Assert.AreSame(expected,actual);
        }

        [TestMethod()]
        public void AsyncJournalingYieldsAsyncWriter()
        {
            var config = new EngineConfiguration(Guid.NewGuid().ToString());
            config.AsyncronousJournaling = true;
            config.SetStoreFactory(c => new InMemoryStore(c));
            var store = config.CreateStore();
            var writer = store.CreateJournalWriter(1);
            Assert.IsTrue(writer is AsynchronousJournalWriter);
        }

        [TestMethod()]
        public void SyncJournalingYieldsSyncWriter()
        {
            var config = new EngineConfiguration(Guid.NewGuid().ToString());
            config.AsyncronousJournaling = false;
            config.SetStoreFactory(c => new InMemoryStore(c));
            var store = config.CreateStore();
            var writer = store.CreateJournalWriter(1);
            Assert.IsTrue(writer is StreamJournalWriter);
        }

        [TestMethod()]
        public void SyncJournalingIsDefault()
        {
            var config = new EngineConfiguration();
            Assert.IsFalse(config.AsyncronousJournaling);
        }

        [TestMethod()]
        public void OptimisticKernelIsDefault()
        {
            var config = new EngineConfiguration();
            Assert.IsTrue(config.Kernel == Kernels.Optimistic);
        }

        [TestMethod()]
        public void OptimisticKernelIsReturned()
        {
            var config = new EngineConfiguration();
            config.Kernel = Kernels.Optimistic;
            var kernel = config.CreateKernel(null);
            Assert.AreEqual(typeof(OptimisticKernel), kernel.GetType());
        }



    }
}
