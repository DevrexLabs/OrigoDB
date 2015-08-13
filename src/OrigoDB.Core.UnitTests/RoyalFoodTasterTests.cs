using System;
using NUnit.Framework;
using OrigoDB.Core.Configuration;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class RoyalFoodTasterTests
    {

        public const int ExpectedState = 42;
        [Serializable]
        public class MyModel : Model
        {
            private int _state = ExpectedState;

            public void MutateAndThrow()
            {
                _state = 13;
                throw new Exception();
            }

            public int GetState()
            {
                return _state;
            }
        }

        [Test]
        public void Corrupting_command_effects_ignored()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            config.Kernel = Kernels.RoyalFoodTaster;
            var db = Engine.For<MyModel>(config).GetProxy();
            Assert.Catch(db.MutateAndThrow);
            Assert.AreEqual(ExpectedState, db.GetState());
        }
    }
}