using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
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

        [Test, Ignore]
        public void MsSqlCommandStoreWriteReadEntries()
        {
            var config = new EngineConfiguration();
            var settings = config.SqlSettings;
            settings.ConnectionString = "Data Source=.;Initial Catalog=fish;Integrated Security=True";
            settings.ProviderName = "System.Data.SqlClient";
            settings.TableName = "[test-" + Guid.NewGuid() + "]";
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
        [Test, Ignore]
        public void MsSqlProviderIntegrationTest()
        {
            var config = new EngineConfiguration();
            config.JournalStorage = StorageType.Sql;
            config.SqlSettings.ConnectionString = "Data Source=.;Initial Catalog=fish;Integrated Security=True;";
            config.SqlSettings.ProviderName = "System.Data.SqlClient";
            var engine = Engine.For<TestModel>(config);
            int initial = engine.Execute(new DelegateQuery<TestModel, int>(m => m.CommandsExecuted));
            engine.Execute(new TestCommandWithoutResult());
            int actual = engine.Execute(new TestCommandWithResult());
            Assert.AreEqual(initial + 2, actual);
            (engine as LocalEngineClient<TestModel>).Engine.Close();
            engine = Engine.For<TestModel>(config);
            actual = engine.Execute(new TestCommandWithResult());
            Assert.AreEqual(initial + 3, actual);
        }
    }
}
