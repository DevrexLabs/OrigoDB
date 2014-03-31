using System;
using System.Diagnostics;
using NUnit.Framework;
using OrigoDB.Core.Proxy;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class ProxyMethodMapTests
    {
        private ProxyMethodMap _map;

        public class TestModel
        {

            public EventHandler<EventArgs> AnEvent;

            public void ImplicitCommand(){}
            
            [Command]
            public int ExplicitCommandWithResult()
            {
                return 42;
            }


            [Query]
            public void ExplicitQuery(out int result)
            {
                result = 42;
            }

            public int ImplicitQuery()
            {
                return 42;
            }


            //[Command(CloneResults)]
            public TestModel MvccOperation()
            {
                return null;
            }
        }


        [SetUp]
        public void Init()
        {
            _map = ProxyMethodMap.MapFor<TestModel>();
        }

        [Test]
        public void Maps_are_cached()
        {
            var map = ProxyMethodMap.MapFor<TestModel>();
            Assert.AreSame(_map, map);
        }

        public void Explicit_command_is_command()
        {
            var target = _map.GetProxyMethodInfo("ExplicitCommandWithResult");
            Assert.IsTrue(target.IsCommand);
            Assert.IsInstanceOf<CommandAttribute>(target.ProxyAttribute);
        }

        [Test]
        public void CommandAttribute_yields_IsCommand()
        {
            var target = _map.GetProxyMethodInfo("ExplicitCommandWithResult");
            Assert.IsTrue(target.IsCommand);
        }

        [Test]
        public void CommandAttribute_is_yielded()
        {
            var target = _map.GetProxyMethodInfo("ExplicitCommandWithResult");
            Assert.IsInstanceOf<CommandAttribute>(target.ProxyAttribute);
        }

        [Test]
        public void QueryAttribute_yields_query()
        {
            var target = _map.GetProxyMethodInfo("ExplicitQuery");
            Assert.IsTrue(target.IsQuery);
            Assert.IsInstanceOf<QueryAttribute>(target.ProxyAttribute);
        }

        [Test]
        public void ImplicitQuery_IsQuery()
        {
            var target = _map.GetProxyMethodInfo("ImplicitQuery");
            Assert.IsTrue(target.IsQuery);
            Assert.IsInstanceOf<QueryAttribute>(target.ProxyAttribute);
        }
    }
}