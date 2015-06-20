using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Core.Test;
using OrigoDB.Models.Redis;

namespace Models.Redis.Tests
{
    public abstract class RedisTestBase
    {
        protected RedisModel _target;

        [SetUp]
        public void Setup()
        {
            //_target = new RedisModel();
            var config = EngineConfiguration.Create().ForIsolatedTest();
            _target = Engine.For<RedisModel>(config).GetProxy();

        }
    }
}