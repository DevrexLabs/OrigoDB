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
        }


        public EngineConfiguration CreateConfig()
        {
            return EngineConfiguration.Create().ForIsolatedTest();
        }


    }
}