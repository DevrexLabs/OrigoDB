using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Core.Test;
using OrigoDB.Core.Modeling.Redis;

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
            _target = Db.For<RedisModel>(config);

        }
    }
}