using System;
using System.IO;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    public class EngineTestBase
    {

        private string _path;

        [SetUp]
        public void Setup()
        {
            _path = Guid.NewGuid().ToString();
        }
        public EngineConfiguration CreateConfig()
        {
            return new EngineConfiguration(_path);
            //return new EngineConfiguration().ForIsolatedTest();
        }

        [TearDown]
        public void TearDown()
        {
            Config.Engines.CloseAll();
            if (Directory.Exists(_path)) Directory.Delete(_path, true);
        }


    }
}