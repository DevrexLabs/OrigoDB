using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using Proxying;

namespace OrigoDB.Core.Test
{
    [TestFixture]
	public class ProxyTest 
	{

        TestModel _proxy;
	    Engine<TestModel> _engine;


        [SetUp]
        public void TestSetup()
        {
            _engine = Engine.Create(new TestModel(), new EngineConfiguration().ForIsolatedTest());
            _proxy = _engine.GetProxy();
        }



	    [Test]
	    public void CanSetProperty()
	    {
	        _proxy.CommandsExecuted = 42;
            Assert.AreEqual(42, _proxy.CommandsExecuted);
	    }


		[Test]
		public void CanExecuteCommandMethod()
		{
			_proxy.IncreaseNumber();
			Assert.AreEqual(1, _proxy.CommandsExecuted);
		}

		[Test]
		public void CanExecuteCommandWithResultMethod()
		{
			Assert.AreEqual(_proxy.Uppercase("livedb"), "LIVEDB");
			Assert.AreEqual(1, _proxy.CommandsExecuted);
		}

		[Test, ExpectedException(typeof(SerializationException))]
		public void ThrowsExceptionOnYieldQuery()
		{
			_proxy.GetNames().Count();
		}

		[Test]
		public void CanExecuteQueryMethod()
		{
			var number = _proxy.GetCommandsExecuted();
			Assert.AreEqual(0, number);
		}

        [Test]
        public void QueryResultsAreCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomers().First();
            Customer robert2 = _proxy.GetCustomers().First();
            Assert.AreNotEqual(robert, robert2);
        }


	    [Test]
        public void SafeQueryResultsAreNotCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomersCloned().First();
            Customer robert2 = _proxy.GetCustomersCloned().First();
            Assert.AreEqual(robert, robert2);
        }

        [Test]
        public void CloneResult_attribute_is_recognized()
        {
            var map = MethodMap.MapFor<TestModel>();
            var signature = typeof (TestModel).GetMethod("GetCustomersCloned").ToString();
            var opInfo = map.GetOperationInfo(signature);
            Assert.False(opInfo.OperationAttribute.CloneResult);

        }
    }
}
