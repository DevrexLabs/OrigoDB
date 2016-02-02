using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using OrigoDB.Core.Journaling;
using OrigoDB.Core.Storage.Sql;

namespace OrigoDB.Core.Test
{

    [TestFixture]
    public class SqlStorageTest
    {

        [Test]
        public void DisplayProviders()
        {
            var table = DbProviderFactories.GetFactoryClasses();
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Console.WriteLine(column.ColumnName + ":" + row[column]);
                }
                Console.WriteLine("-------------------------------------------------");
            }
        }

        private IEnumerable<SqlSettings>  Providers()
        {
            yield return new SqlSettings
            {
                ProviderName = "System.Data.OleDb",
                ConnectionString = "Data Source=.;Initial Catalog=fish;Integrated Security=SSPI;Provider=SQLOLEDB;"
            };
            yield return new SqlSettings
            {
                ProviderName = "System.Data.SqlClient",
                ConnectionString = "Data Source=.;Initial Catalog=fish;Integrated Security=True;"
            };
        }

        [Test, TestCaseSource("Providers"), Ignore]
        public void SqlCommandStoreWriteReadEntries(SqlSettings settings)
        {
            var config = new EngineConfiguration();
            config.SqlSettings = settings;
            settings.TableName = "test-" + Guid.NewGuid();
            var commandStore = new SqlCommandStore(config);
            commandStore.Initialize();
            var formatter = new BinaryFormatter();
            var writer = new SqlJournalWriter(formatter, commandStore);
            writer.Write(JournalEntry.Create(1UL, DateTime.Now, new ModelCreated(typeof (TestModel))));
            writer.Write(JournalEntry.Create(2UL, DateTime.Now.AddSeconds(1), new AppendNumberCommand(42)));
            writer.Write(JournalEntry.Create(3UL, DateTime.Now.AddSeconds(2), new AppendNumberCommand(64)));

            foreach (var entry in commandStore.GetJournalEntriesFrom(1))
            {
                Trace.WriteLine(entry);
            }
            writer.Dispose();
        }

        [Test, TestCaseSource("Providers"), Ignore]
        public void TruncateJournal(SqlSettings settings)
        {
            var config = new EngineConfiguration();
            config.SqlSettings = settings;
            settings.TableName = "test-" + Guid.NewGuid();
            var commandStore = new SqlCommandStore(config);
            commandStore.Initialize();
            var formatter = new BinaryFormatter();
            var writer = new SqlJournalWriter(formatter, commandStore);
            Enumerable.Range(1, 50).ToList().ForEach(i =>
            {
                writer.Write(JournalEntry.Create((ulong)i, DateTime.Now.AddSeconds(i), new AppendNumberCommand(i)));
            });
            writer.Dispose();
            commandStore.Truncate(40);
            Assert.AreEqual(10, commandStore.CommandEntries().Count());
        }

        [Test, TestCaseSource("Providers"), Ignore]
        public void Initstatement_is_idempotent(SqlSettings settings)
        {
            var config = new EngineConfiguration();
            config.JournalStorage = StorageType.Sql;
            config.SqlSettings = settings;
            config.SqlSettings.TableName = "test-" + Guid.NewGuid();
            
            config.CreateCommandStore();
            config.CreateCommandStore();
        }

        [Test, TestCaseSource("Providers"), Ignore]
        public void SqlProviderIntegrationTest(SqlSettings settings)
        {
            var config = new EngineConfiguration();
            config.JournalStorage = StorageType.Sql;
            config.SqlSettings = settings;
            config.SqlSettings.TableName = "test-" + Guid.NewGuid();

            config.SnapshotPath = Guid.NewGuid().ToString();
            config.TruncateJournalOnSnapshot = true;
            var engine = Engine.For<TestModel>(config);
            int initial = engine.Execute(new DelegateQuery<TestModel, int>(m => m.CommandsExecuted));
            engine.Execute(new TestCommandWithoutResult());
            int actual = engine.Execute(new TestCommandWithResult());
            Assert.AreEqual(initial + 2, actual);
            Config.Engines.CloseAll();
            engine = Engine.For<TestModel>(config);
            actual = engine.Execute(new TestCommandWithResult());
            Assert.AreEqual(initial + 3, actual);
            Config.Engines.All.First().CreateSnapshot();

            var store = config.CreateCommandStore();
            Assert.AreEqual(0, store.CommandEntries().Count(), "journal should be empty after snapshot");
            Config.Engines.CloseAll();
            engine = Engine.For<TestModel>(config);
            int commandsExecuted = engine.Execute(new DelegateQuery<TestModel, int>(m => m.CommandsExecuted));
            Assert.AreEqual(commandsExecuted, actual, "state should be same after close and reload");
            Config.Engines.CloseAll();

            //cleanup
            Directory.Delete(config.SnapshotPath, true);
        }
    }
}
