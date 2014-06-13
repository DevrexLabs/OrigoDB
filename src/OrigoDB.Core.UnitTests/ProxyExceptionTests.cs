using System;
using NUnit.Framework;
using OrigoDB.Core.Proxy;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class ProxyExceptionTests
    {
        [Test]
        public void ExceptionInProxiedMethodIsPropagated()
        {
            var cfg = EngineConfiguration.Create().ForIsolatedTest();
            var engine = Engine.Create<ProxyExceptionTestModel>(cfg);
            var proxy = engine.GetProxy();
            int callsToExecutíng = 0, callsToExecuted = 0;
            engine.CommandExecuting += (sender, args) => callsToExecutíng++;
            engine.CommandExecuted += (sender, args) => callsToExecuted++;
            Assert.Throws<CommandAbortedException>(
                () => proxy.Throw(new CommandAbortedException())
                );
            Assert.AreEqual(1, callsToExecutíng);
            Assert.AreEqual(0, callsToExecuted);
        }        
    }

    [Serializable]
    public class ProxyExceptionTestModel : Model
    {
        public void Throw(Exception ex)
        {
            throw ex;
        }
    }
}