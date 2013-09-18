using System;
using System.IO;
using System.Linq;
using System.Threading;
using OrigoDB.Core.Logging;
using OrigoDB.Modules.SqlStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrigoDB.Core.Test
{
    [TestClass]
    public class EngineTestBase
    {

        public Engine Engine { get; set; }
        public String Path { get; set; }


	    [TestInitialize]
        public void MyTestInitialize() 
        {
            Path = Guid.NewGuid().ToString();
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
        
        }

        /// <summary>
        /// modify this method to switch between different configurations
        /// todo: use
        /// </summary>
        /// <returns></returns>
        public EngineConfiguration CreateConfig()
        {
            return CreateNonPersistingConfig();
        }

        public EngineConfiguration CreateNonPersistingConfig()
        {
            var config = new EngineConfiguration();
            config.SetStoreFactory(cfg => new InMemoryStore(config));
            config.Location.OfJournal = Path;
            return config;
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