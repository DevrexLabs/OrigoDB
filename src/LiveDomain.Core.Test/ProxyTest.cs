using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiveDomain.Core.Proxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveDomain.Core.Test
{
	[TestClass]
	public class ProxyTest : EngineTestBase
	{
		Engine<TestModel> _engine; 
		TestModel _proxy;


		[TestMethod]
		public void CanCreateEngine()
		{
			_engine = Engine.Create(new TestModel(), CreateConfig());
        }


        [TestMethod]
        public void CanCloneMarshalByRefModel()
        {
            var model = new TestModel();
            model.AddCustomer("Zippy");
            var serializer = new Serializer(new BinaryFormatter());
            var clone = serializer.Clone(model);
            model.AddCustomer("asfafse");
            Assert.IsTrue(clone.Customers.Count() == 1);
        }


		[TestMethod]
		public void CanCreateProxy()
		{
			CanCreateEngine();
			_proxy = _engine.GetProxy();
		}

		[TestMethod]
		public void CanExecuteCommandMethod()
		{
			CanCreateProxy();
			_proxy.IncreaseNumber();
			Assert.AreEqual(1, _proxy.CommandsExecuted);
		}

		[TestMethod]
		public void CanExecuteCommandWithResultMethod()
		{
			CanCreateProxy();
			Assert.AreEqual(_proxy.Uppercase("livedb"), "LIVEDB");
			Assert.AreEqual(1, _proxy.CommandsExecuted);
		}

		[TestMethod, ExpectedException(typeof(SerializationException))]
		public void ThrowsExceptionOnYieldQuery()
		{
			CanCreateProxy();
			int i = _proxy.GetNames().Count();
			Assert.IsTrue(i == 10);
		}

		[TestMethod]
		public void CanExecuteQueryMethod()
		{
			CanCreateProxy();
			var number = _proxy.GetCommandsExecuted();
			Assert.AreEqual(number,0);
		}

        [TestMethod]
        public void QueryResultsAreCloned()
        {
            CanCreateProxy();
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomers().First();
            Customer robert2 = _proxy.GetCustomers().First();
            Assert.AreNotEqual(robert, robert2);
        }


        [TestMethod]
        public void IntResultIsNotCloned()
        {
            CanCreateProxy();
            _proxy.AddCustomer("Robert");
            _logger.Clear();
            int commandExecuted = _proxy.GetCommandsExecuted();
            Assert.IsTrue(_logger.Messages.Count(m => m.IndexOf("Cloned") >= 0) == 0);

        }

	    [TestMethod]
        public void SafeQueryResultsAreNotCloned()
        {
            CanCreateProxy();
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomersCloned().First();
            Customer robert2 = _proxy.GetCustomersCloned().First();
            Assert.AreEqual(robert, robert2);
        }

    }
}
