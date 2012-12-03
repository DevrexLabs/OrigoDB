using LiveDomain.Core;
using LiveDomain.Core.Configuration;
using LiveDomain.Core.Journaling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.Serialization;
using System.IO;
using LiveDomain.Core.Security;
using System.Runtime.Serialization.Formatters.Binary;
using LiveDomain.Core.Storage;

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
            Assert.AreSame(expected, actual);
        }
        
        [TestMethod()]
        public void InjectingStorageSetsModeToCustom()
        {
            var config = new EngineConfiguration();
            config.SetStoreFactory((c) => null);
            Assert.AreEqual(StoreType.Custom, config.StoreType);
        }

        [TestMethod()]
        public void FileStorageIsDefault()
        {
            var config = new EngineConfiguration();
            var storage = config.CreateStore();
            Assert.IsTrue(storage is FileStore);
        }

        [TestMethod()]
        public void InjectedStorageIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new FileStore(config);
            config.SetStoreFactory((c) => expected);
            var actual = config.CreateStore();
            Assert.AreSame(expected, actual);
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
        public void InjectedSerializerIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new Serializer(new BinaryFormatter());
            config.SetSerializerFactory((c) => expected);
            var actual = config.CreateSerializer();
            Assert.AreSame(expected, actual);
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

        class MockStore : Store
        {
            public MockStore(EngineConfiguration config) : base(config)
            {
                
            }

            protected override IJournalWriter CreateStoreSpecificJournalWriter(long lastEntryId)
            {
                return new NullJournalWriter();
            }

            protected override Snapshot WriteSnapshotImpl(Model model, long lastEntryId)
            {
                throw new NotImplementedException();
            }

            public override System.Collections.Generic.IEnumerable<JournalEntry> GetJournalEntriesFrom(long sequenceNumber)
            {
                yield break;
            }

            public override System.Collections.Generic.IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
            {
                yield break;
            }

            public override Model LoadMostRecentSnapshot(out long lastSequenceNumber)
            {
                lastSequenceNumber = 1;
                return null;
            }

            public override void VerifyCanLoad()
            {
                
            }

            public override void VerifyCanCreate()
            {
                
            }

            public override void Create(Model model)
            {
               
            }

            protected override System.Collections.Generic.IEnumerable<Snapshot> LoadSnapshots()
            {
                yield break;
            }
        }
        [TestMethod()]
        public void AsyncJournalingYieldsAsyncWriter()
        {
            var config = new EngineConfiguration();
            config.AsyncronousJournaling = true;
            config.SetStoreFactory(c => new MockStore(c));
            var store = config.CreateStore();
            var writer = store.CreateJournalWriter(1);
            Assert.IsTrue(writer is AsynchronousJournalWriter);
        }

        [TestMethod()]
        public void SyncJournalingYieldsSyncWriter()
        {
            var config = new EngineConfiguration();
            config.AsyncronousJournaling = false;
            config.SetStoreFactory(c => new MockStore(c));
            var store = config.CreateStore();
            var writer = store.CreateJournalWriter(1);
            Assert.IsTrue(writer is NullJournalWriter);
        }

        [TestMethod()]
        public void SyncJournalingIsDefault()
        {
            var config = new EngineConfiguration();
            Assert.IsFalse(config.AsyncronousJournaling);
        }

        [TestMethod()]
        public void PessimisticKernelIsDefault()
        {
            var config = new EngineConfiguration();
            Assert.IsTrue(config.KernelMode == KernelMode.Pessimistic);
        }

        [TestMethod()]
        public void PessimisticKernelIsReturned()
        {
            var config = new EngineConfiguration();
            config.KernelMode = KernelMode.Pessimistic;
            var kernel = config.CreateKernel(null);
            Assert.AreEqual(typeof(PessimisticKernel), kernel.GetType());
        }

        [TestMethod()]
        public void OptimisticKernelIsReturned()
        {
            var config = new EngineConfiguration();
            config.KernelMode = KernelMode.Optimistic;
            var kernel = config.CreateKernel(null);
            Assert.AreEqual(typeof(OptimisticKernel), kernel.GetType());
        }



    }
}
