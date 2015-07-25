using System;
using NUnit.Framework;
using OrigoDB.Core;

namespace OrigoDB.Core.Test
{
    public class EngineTestBase
    {

        public EngineConfiguration CreateConfig()        {
            return EngineConfiguration
                .Create().ForIsolatedTest();
        }


    }
}