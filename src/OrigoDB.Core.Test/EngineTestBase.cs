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
			Config.Engines.CloseAll();
        }


        public EngineConfiguration CreateConfig()
        {
            var config = new EngineConfiguration();
            var store = new InMemoryStore();
            config.SetStoreFactory(cfg => store);
            config.Location.OfJournal = Path;
            return config;
        }


    }
}