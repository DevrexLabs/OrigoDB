using System;
using System.Configuration;
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
        [Test, Ignore]
        public void MsSqlProviderSmokeTest()
        {
            ConnectionStringSettings settings = new ConnectionStringSettings("fish",
                "Data Source=.;Initial Catalog=fish;Integrated Security=True", "System.Data.SqlClient");
            var provider = SqlProvider.Create(settings, "fish");
            provider.Initialize();
            var formatter = new BinaryFormatter();
            var writer = new SqlJournalWriter(formatter, provider);
            writer.Write(JournalEntry.Create(1UL, DateTime.Now, new ModelCreated(typeof (TestModel))));
            writer.Write(JournalEntry.Create(2UL, DateTime.Now.AddSeconds(1), new AppendNumberCommand(42)));
            writer.Write(JournalEntry.Create(3UL, DateTime.Now.AddSeconds(2), new AppendNumberCommand(64)));

            foreach (var entry in provider.ReadJournalEntries(1, bytes => formatter.FromByteArray<object>(bytes)))
            {
                Console.WriteLine(entry.GetItem());
            }
            writer.Dispose();
        }
        [Test, Ignore]
        public void MsSqlProviderIntegrationTest()
        {
            var config = EngineConfiguration.Create();
            config.JournalStorage = StorageType.Sql;
            config.Location.OfJournal = "Data Source=.;Initial Catalog=fish;Integrated Security=True;";
            config.Location.OfSnapshots = "dish";
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
