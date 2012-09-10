using System;
using System.IO;
using System.Threading;
using LiveDomain.Core.Logging;
using LiveDomain.Modules.SqlStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveDomain.Core.Test
{
    [TestClass]
    public class EngineTestBase
    {
        protected static InMemoryLogger _logger = new InMemoryLogger();

        static EngineTestBase()
        {
            Log.SetLogFactory(new SingletonLogFactory(_logger));
        }

        public Engine Engine { get; set; }
        public String Path { get; set; }

        [TestInitialize]
        public void MyTestInitialize() 
        {
            Path = Guid.NewGuid().ToString();
            _logger.Clear();
        }

        public void WriteLog()
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine(" LOG");
            Console.WriteLine("------------------------------");
            foreach (var message in _logger.Messages)
            {
                Console.WriteLine((string) message);
            }
        }

        [TestCleanup()]
        public void MyTestCleanup() 
        {
            if (Engine != null)
            {
                Engine.Close();
                Thread.Sleep(50);
                if (Directory.Exists(Path)) new DirectoryInfo(Path).Delete(true);
            }
            Console.WriteLine("Path:" + Path);
            WriteLog();
        }

        /// <summary>
        /// modify this method to switch between sql and file store tests
        /// </summary>
        /// <returns></returns>
        public EngineConfiguration CreateConfig()
        {
            //return CreateSqlConfig();
            return CreateFileConfig();
        }

        private EngineConfiguration CreateFileConfig()
        {
            var config = new EngineConfiguration();

            //Connection string name in app.config file
            config.Location = Path;
            config.SnapshotBehavior = SnapshotBehavior.None;
            config.Synchronization = SynchronizationMode.ReadWrite;
            return config;
            
        }

        protected SqlEngineConfiguration CreateSqlConfig()
        {
            var config = new SqlEngineConfiguration();
            
            //Connection string name in app.config file
            config.Location = "livedbstorage";
            config.SnapshotLocation = Path;

            //new table for every test. Cleanup your test database later
            config.JournalTableName = Path;

            config.SnapshotBehavior = SnapshotBehavior.None;
            config.Synchronization = SynchronizationMode.ReadWrite;
            return config;
        }

        protected void DeleteFromDefaultLocation<M>() where M : Model
        {
            var config = new EngineConfiguration();
            config.SetLocationFromType<M>();
            var dirInfo = new DirectoryInfo(config.Location);
            if (dirInfo.Exists)
            {
                dirInfo.Delete(recursive: true);
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }
        }
    }
}