using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using OrigoDB.Core.Proxying;

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
	        int flag = 0;
	        _engine.CommandExecuted += (sender, args) => flag ++;
	        _proxy.CommandsExecuted = 42;
            Assert.AreEqual(1, flag);
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

        [Test]
        public void GenericQuery()
        {
            var customer = new Customer();
            var clone = _proxy.GenericQuery(customer);
            Assert.AreNotSame(clone,customer);
            Assert.IsInstanceOf<Customer>(clone);
        }

        [Test]
        public void GenericCommand()
        {
            _proxy.GenericCommand(DateTime.Now);
            Assert.AreEqual(1, _proxy.CommandsExecuted);
        }

        [Test]
        public void ComplexGeneric()
        {
            double result = _proxy.ComplexGeneric(new KeyValuePair<string, double>("dog", 42.0));
            Assert.AreEqual(result, 42.0, 0.0001);
            Assert.AreEqual(1, _proxy.CommandsExecuted);
        }

        [Test]
        public void Indexer()
        {
            _proxy.AddCustomer("Homer");
            Assert.AreEqual(1, _proxy.CommandsExecuted);

            var customer = _proxy[0];
            Assert.AreEqual("Homer", customer.Name);

            customer.Name = "Bart";
            _proxy[0] = customer;
            Assert.AreEqual(2, _proxy.CommandsExecuted);
            var customers = _proxy.GetCustomers();
            Assert.True(customers.Single().Name == "Bart");
        }

        [Test]
        public void DefaultArgs()
        {
            var result = _proxy.DefaultArgs(10, 10);
            Assert.AreEqual(62, result);

            result = _proxy.DefaultArgs(10, 10, 10);
            Assert.AreEqual(30,result);
        }

        [Test]
        public void NamedArgs()
        {
            var result = _proxy.DefaultArgs(b: 4, a: 2);
            Assert.AreEqual(48, result);
        }

        [Test]
        public void ExplicitGeneric()
        {
            var dt = _proxy.ExplicitGeneric<DateTime>();
            Assert.IsInstanceOf<DateTime>(dt);
            Assert.AreEqual(default(DateTime), dt);
        }
    }
}
