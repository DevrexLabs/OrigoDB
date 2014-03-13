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

	    [TestInitialize]
        public void MyTestInitialize() 
        {

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
            return config;
        }


    }
}