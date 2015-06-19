using NUnit.Framework;
using OrigoDB.Core.Proxy;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class MethodMapTests
    {
        private MethodMap _map;

        public class TestModel
        {

            public void ImplicitCommand(){}
            
            [Command]
            public int ExplicitCommandWithResult()
            {
                return 42;
            }

            [NoProxy]
            public int NoProxyQuery()
            {
                return 4;
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

            [Command(CloneResult = false)]
            public TestModel MvccOperation()
            {
                return null;
            }
        }


        [SetUp]
        public void Init()
        {
            _map = MethodMap.MapFor<TestModel>();
        }

        [Test]
        public void Maps_are_cached()
        {
            var map = MethodMap.MapFor<TestModel>();
            Assert.AreSame(_map, map);
        }

        [Test]
        public void Implicit_command_IsCommand()
        {
            var target = _map.GetProxyMethodInfo("ImplicitCommand");
            Assert.IsTrue(target.IsCommand);
        }

        [Test]
        public void Explicit_command_IsCommand()
        {
            var target = _map.GetProxyMethodInfo("ExplicitCommandWithResult");
            Assert.IsTrue(target.IsCommand);
        }

        [Test]
        public void Explicit_query_IsQuery()
        {
            var target = _map.GetProxyMethodInfo("ExplicitQuery");
            Assert.IsTrue(target.IsQuery);
        }

        [Test]
        public void Implicit_query_IsQuery()
        {
            var target = _map.GetProxyMethodInfo("ImplicitQuery");
            Assert.IsTrue(target.IsQuery);
        }

        [Test]
        public void NoProxy_is_disallowed()
        {
            var target = _map.GetProxyMethodInfo("NoProxyQuery");
            Assert.IsFalse(target.IsAllowed);
        }

        [Test]
        public void CloneResults_is_default_implicit()
        {
            var target = _map.GetProxyMethodInfo("ImplicitQuery");
            Assert.IsTrue(target.ProxyAttribute.CloneResult);
        }

        [Test]
        public void CloneResult_is_default_for_explicit()
        {
            var target = _map.GetProxyMethodInfo("ExplicitCommandWithResult");
            Assert.IsTrue(target.ProxyAttribute.CloneResult);
        }

        [Test]
        public void Explicit_CloneResult_is_reported()
        {
            var target = _map.GetProxyMethodInfo("MvccOperation");
            Assert.IsFalse(target.ProxyAttribute.CloneResult);
        }

    }
}