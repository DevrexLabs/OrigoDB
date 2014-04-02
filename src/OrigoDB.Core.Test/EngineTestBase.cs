using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrigoDB.Core.Test
{
    [TestClass]
    public class EngineTestBase
    {

        public EngineConfiguration CreateConfig()        {
            return EngineConfiguration
                .Create().ForIsolatedTest();
        }


    }
}