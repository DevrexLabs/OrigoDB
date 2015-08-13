using NUnit.Framework;
using OrigoDB.Core.Configuration;
using System;
using System.IO;
using OrigoDB.Core.Security;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class EngineLoadTest
    {
        [Test]
        public void CanLoadAndCreateRepeatedly()
        {
            var config = EngineConfiguration.Create().WithRandomLocation();
            try
            {
                var engine = Engine.LoadOrCreate<TestModel>(config);
                engine.Execute(new TestCommandWithResult());
                engine.Close();
                engine = Engine.LoadOrCreate<TestModel>(config);
                engine.Close();
            }
            finally
            {
                Thread.Sleep(40);
                if (config.CreateCommandStore() is FileCommandStore)
                    Directory.Delete(config.Location.OfJournal, true);    
            }
        }
    }

    [TestFixture]
    public class EngineConfigurationTest
    {
        [Test]
        public void InjectedFormatterIsResolved()
        {
            var config = new EngineConfiguration();
            config.PacketOptions = null;
            var expected = new BinaryFormatter();
            config.SetFormatterFactory((c,f) => expected);
            var actual = config.CreateFormatter(FormatterUsage.Default);
            Assert.AreSame(expected, actual);
        }

        [Test]
        public void PacketingFormatterIsReturnedWhenPaketOptionsArePresent()
        {
            var config = new EngineConfiguration();
            config.PacketOptions = PacketOptions.Checksum;
            config.SetFormatterFactory((c,f) => new BinaryFormatter());
            var actual = config.CreateFormatter(FormatterUsage.Journal);
            Assert.IsInstanceOf<PacketingFormatter>(actual);
        }

        [Test]
        public void FileStorageIsDefault()
        {
            var config = new EngineConfiguration().WithRandomLocation();
            var storage = config.CreateCommandStore();
            Assert.IsTrue(storage is FileCommandStore);
            Directory.Delete(config.Location.OfJournal, true);
        }

        [Test]
        public void InjectedStorageIsResolved()
        {
            var config = new EngineConfiguration()
                .WithRandomLocation();
            var expected = new FileCommandStore(config);
            config.SetCommandStoreFactory((c) => expected);
            var actual = config.CreateCommandStore();
            Assert.AreSame(expected, actual);
            Directory.Delete(config.Location.OfJournal, true);
        }

        [Test]
        public void SynchronizerDefaultsToReadWrite()
        {
            var config = new EngineConfiguration();
            var synchronizer = config.CreateSynchronizer();
            Assert.IsTrue(synchronizer is ReadWriteSynchronizer);
        }

        [Test]
        public void InjectingSynchronizerSetsModeToCustom()
        {
            var config = new EngineConfiguration();
            config.SetSynchronizerFactory((c) => null);
            Assert.AreEqual(SynchronizationMode.Custom, config.Synchronization);
        }

        [Test]
        public void InjectedSynchronizerIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new NullSynchronizer();
            config.SetSynchronizerFactory((c) => expected);
            var actual = config.CreateSynchronizer();
            Assert.AreSame(expected,actual);
        }

        [Test]
        public void InjectedAuthorizerIsResolved()
        {
            var config = new EngineConfiguration();
            var expected = new Authorizer();
            config.SetAuthorizerFactory((c) => expected);
            var actual = config.CreateAuthorizer();
            Assert.AreSame(expected,actual);
        }

        [Test]
        public void AsyncJournalingYieldsAsyncWriter()
        {
            var config = new EngineConfiguration(Guid.NewGuid().ToString());
            config.AsynchronousJournaling = true;
            config.SetCommandStoreFactory(c => new InMemoryCommandStore(c));
            var store = config.CreateCommandStore();
            var writer = store.CreateJournalWriter(1);
            Assert.IsTrue(writer is AsynchronousJournalWriter);
        }

        [Test]
        public void SyncJournalingYieldsSyncWriter()
        {
            var config = new EngineConfiguration(Guid.NewGuid().ToString());
            config.AsynchronousJournaling = false;
            config.SetCommandStoreFactory(c => new InMemoryCommandStore(c));
            var store = config.CreateCommandStore();
            var writer = store.CreateJournalWriter(1);
            Assert.IsTrue(writer is StreamJournalWriter);
        }

        [Test]
        public void SyncJournalingIsDefault()
        {
            var config = new EngineConfiguration();
            Assert.IsFalse(config.AsynchronousJournaling);
        }

        [Test]
        public void OptimisticKernelIsDefault()
        {
            var config = new EngineConfiguration();
            Assert.IsTrue(config.Kernel == Kernels.Optimistic);
        }

        [Test]
        public void OptimisticKernelIsReturned()
        {
            var config = new EngineConfiguration();
            config.Kernel = Kernels.Optimistic;
            var kernel = config.CreateKernel(null);
            Assert.AreEqual(typeof(OptimisticKernel), kernel.GetType());
        }
    }
}
