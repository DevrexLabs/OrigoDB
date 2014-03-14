using System;
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
			//Config.Engines.CloseAll();
        }


        public EngineConfiguration CreateConfig()
        {
            var randomLocation = Guid.NewGuid().ToString();
            var config = new EngineConfiguration(randomLocation);
            config.SetStoreFactory(cfg => new InMemoryStore(cfg));
            return config;
        }


    }
}