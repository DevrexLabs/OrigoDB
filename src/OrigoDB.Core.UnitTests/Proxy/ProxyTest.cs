using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core.Proxy;


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
        public void CanCloneMarshalByRefModel()
        {
            var model = new TestModel();
            model.AddCustomer("Zippy");
            
            var clone = new BinaryFormatter().Clone(model);

            //modify the original which should not effect the clone
            model.AddCustomer("asfafse");
            Assert.IsTrue(clone.Customers.Count() == 1);
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
			int i = _proxy.GetNames().Count();
			Assert.IsTrue(i == 10);
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
    }
}
