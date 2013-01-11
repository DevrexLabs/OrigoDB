using System;
using System.IO;
using System.Linq;
using System.Threading;
using LiveDomain.Core.Logging;
using LiveDomain.Modules.SqlStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveDomain.Core.Test
{
    [TestClass]
    public class EngineTestBase
    {
        protected MemorySink _memoryLogWriter = new MemorySink();


        public Engine Engine { get; set; }
        public String Path { get; set; }


	    [TestInitialize]
        public void MyTestInitialize() 
        {
            Path = Guid.NewGuid().ToString();
            
            (LogProvider.Factory as LogFactory).Kernel.AddWriter(_memoryLogWriter);
            _memoryLogWriter.Clear();
            
        }

        public void WriteLog()
        {

            if (!_memoryLogWriter.Messages.Any()) return;
            Console.WriteLine("------------------------------");
            Console.WriteLine(" LOG");
            Console.WriteLine("------------------------------");
            foreach (var message in _memoryLogWriter.Messages)
            {
                Console.WriteLine((string) message);
            }
        }

        [TestCleanup()]
        public void MyTestCleanup() 
        {
			//Clear cached engines
			Config.Engines.CloseAll();

            if (Engine != null)
            {
                Engine.Close();
                Thread.Sleep(50);
                if (Directory.Exists(Path)) new DirectoryInfo(Path).Delete(true);
            }
            Console.WriteLine("Path:" + Path);
            WriteLog();

            (LogProvider.Factory as LogFactory).Kernel.RemoveWriter(_memoryLogWriter);
        }

        /// <summary>
        /// modify this method to switch between sql and file store tests
        /// </summary>
        /// <returns></returns>
        public EngineConfiguration CreateConfig()
        {
            return CreateFileConfig();
        }

        private EngineConfiguration CreateFileConfig()

        {
            var config = EngineConfiguration.Create();

            //Connection string name in app.config file
            config.Location.OfJournal = Path;
            config.SnapshotBehavior = SnapshotBehavior.None;
            config.Synchronization = SynchronizationMode.ReadWrite;
            return config;
            
        }

        protected SqlEngineConfiguration CreateSqlConfig()
        {
            var config = new SqlEngineConfiguration();
            
            //Connection string name in app.config file
            config.Location.OfJournal = "livedbstorage";
            config.Location.OfSnapshots = Path;

            //new table for every test. Cleanup your test database later
            config.JournalTableName = Path;

            config.SnapshotBehavior = SnapshotBehavior.None;
            config.Synchronization = SynchronizationMode.ReadWrite;
            return config;
        }

        protected void DeleteFromDefaultLocation<M>() where M : Model
        {
            var config = new EngineConfiguration();
            config.Location.SetLocationFromType<M>();
            var dirInfo = new DirectoryInfo(config.Location.OfJournal);
            if (dirInfo.Exists)
            {
                dirInfo.Delete(recursive: true);
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
            }
        }
    }
}