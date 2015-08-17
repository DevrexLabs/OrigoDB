using NUnit.Framework;
using OrigoDB.Core.Proxying;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class MethodMapTests
    {
        private MethodMap<TestModel> _map;

        public class TestModel : Model
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

            public int ImplicitQuery()
            {
                return 42;
            }

            public void GenericCommand<T>(T item)
            {
                
            }

            public T GenericQuery<T>(T item)
            {
                return item;
            }

            [Command]
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
            var target = _map. GetOperationInfo("ImplicitCommand");
            Assert.IsTrue(target is CommandInfo<TestModel>);
        }

        [Test]
        public void Explicit_command_IsCommand()
        {
            var target = _map.GetOperationInfo("ExplicitCommandWithResult");
            Assert.IsTrue(target is CommandInfo<TestModel>);
        }

        [Test]
        public void Implicit_query_IsQuery()
        {
            var target = _map.GetOperationInfo("ImplicitQuery");
            Assert.IsTrue(target is QueryInfo<TestModel>);
        }

        [Test]
        public void NoProxy_is_disallowed()
        {
            var target = _map.GetOperationInfo("NoProxyQuery");
            Assert.IsFalse(target.IsAllowed);
        }

        [Test]
        public void Default_ResultIsIsolated_is_false_implicit()
        {
            var target = _map.GetOperationInfo("ImplicitQuery");
            Assert.IsFalse(target.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

        [Test]
        public void Default_ResultIsIsolated_is_false_for_explicit()
        {
            var target = _map.GetOperationInfo("ExplicitCommandWithResult");
            Assert.IsFalse(target.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

        [Test]
        public void Explicit_ResultIsIsolated_is_reported()
        {
            var target = _map.GetOperationInfo("MvccOperation");
            Assert.IsFalse(target.OperationAttribute.Isolation.HasFlag(IsolationLevel.Output));
        }

    }
}