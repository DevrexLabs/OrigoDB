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

        TestModel _proxy;
	    Engine<TestModel> _engine;


        [TestInitialize]
        public void TestSetup()
        {
            Engine =_engine = Engine.Create(new TestModel(), CreateConfig());
            _proxy = _engine.GetProxy();
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
		public void CanExecuteCommandMethod()
		{
			_proxy.IncreaseNumber();
			Assert.AreEqual(1, _proxy.CommandsExecuted);
		}

		[TestMethod]
		public void CanExecuteCommandWithResultMethod()
		{
			Assert.AreEqual(_proxy.Uppercase("livedb"), "LIVEDB");
			Assert.AreEqual(1, _proxy.CommandsExecuted);
		}

		[TestMethod, ExpectedException(typeof(SerializationException))]
		public void ThrowsExceptionOnYieldQuery()
		{
			int i = _proxy.GetNames().Count();
			Assert.IsTrue(i == 10);
		}

		[TestMethod]
		public void CanExecuteQueryMethod()
		{
			var number = _proxy.GetCommandsExecuted();
			Assert.AreEqual(number,0);
		}

        [TestMethod]
        public void QueryResultsAreCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomers().First();
            Customer robert2 = _proxy.GetCustomers().First();
            Assert.AreNotEqual(robert, robert2);
        }


        [TestMethod]
        public void IntResultIsNotCloned()
        {
            _proxy.AddCustomer("Robert");
            _memoryLogWriter.Clear();
            int commandExecuted = _proxy.GetCommandsExecuted();
            Assert.IsTrue(_memoryLogWriter.Messages.Count(m => m.IndexOf("Cloned") >= 0) == 0);

        }

	    [TestMethod]
        public void SafeQueryResultsAreNotCloned()
        {
            _proxy.AddCustomer("Robert");
            Customer robert = _proxy.GetCustomersCloned().First();
            Customer robert2 = _proxy.GetCustomersCloned().First();
            Assert.AreEqual(robert, robert2);
        }

    }
}
