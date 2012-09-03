using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiveDomain.Core.Proxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveDomain.Core.Test
{
	[TestClass]
	public class ProxyTest
	{
		Engine<TestModel> _engine;
		TestModel _proxy;
		InMemoryLogger _logger;
		public string Path { get; set; }

		[TestInitialize]
		public void MyTestInitialize()
		{
			_logger = new InMemoryLogger();
			//Log.SetLogger(_logger);
			Path = Guid.NewGuid().ToString();
		}

		public EngineConfiguration CreateConfig()
		{
			var config = new EngineConfiguration();
			config.Location = Path;
			config.SnapshotBehavior = SnapshotBehavior.AfterRestore;
			config.Synchronization = SynchronizationMode.Exclusive;
			return config;
		}

		[TestMethod]
		public void CanCreateEngine()
		{
			_engine = Engine.Create(new TestModel(), CreateConfig());
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
			var number = _proxy.GetNumber();
			Assert.AreEqual(number,0);
		}

		[TestCleanup()]
		public void MyTestCleanup()
		{
			if (_engine != null)
			{
				_engine.Close();
				Thread.Sleep(500);
				if (Directory.Exists(Path)) new DirectoryInfo(Path).Delete(true);
			}
		}
	}
}
