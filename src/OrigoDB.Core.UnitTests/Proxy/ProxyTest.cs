using System;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using OrigoDB.Core.Proxying;

namespace OrigoDB.Core.Test
{

    public static class ModelExtensions
    {
        //Generics wont proxy. this is a workaround.
        public static T Add<T>(this ProxyTest.MyModel model, T item)
        {
            return (T)model.Add(item.GetType(), item);
        }
    }

    [TestFixture]
	public class ProxyTest 
	{
        [Serializable]
        public class MyModel : Model
        {
            public int Calls { get; private set; }

            [Command]
            public object Add(Type t, object item)
            {
                Calls++;
                return item;
            }
        }

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

        [Test,Ignore]
        public void GenericQuery()
        {
            var customer = new Customer();
            var clone = _proxy.GenericQuery(customer);
            Assert.AreNotSame(clone,customer);
            Assert.IsInstanceOf<Customer>(clone);
        }

        [Test]
        public void Generics_using_extension_methods()
        {
            var config = new EngineConfiguration().ForIsolatedTest();
            var proxy = Db.For<MyModel>(config);
            proxy.Add(new Customer());
            Assert.AreEqual(1, proxy.Calls);
        }
    }
}
