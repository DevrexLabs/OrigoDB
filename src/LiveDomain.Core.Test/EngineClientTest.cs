using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveDomain.Core.Test
{
	[TestClass]
	public class EngineClientTest
	{
		static IEngine<TestModel> _engine;
		static string _path = "c:\\db\\engineForDb";
		static string _pathForConnectionString = "c:\\db\\engineForDb_2";

		[TestMethod]
		public void CanCreateLocalEngineClientFromConnectionString()
		{
			var engine = Engine.For<TestModel>("mode=embedded;location=" + _pathForConnectionString);
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine, typeof(ILocalEngine<TestModel>));
		}

		[TestMethod]
		public void CanCreateLocalEngineClientFromConfig()
		{
			if (_engine != null)
			{
				Assert.IsInstanceOfType(_engine, typeof(ILocalEngine<TestModel>));
				return;
			}
			_engine = Engine.For<TestModel>(CreateFileConfig());
			Assert.IsNotNull(_engine);
			Assert.IsInstanceOfType(_engine, typeof(ILocalEngine<TestModel>));
		}

		[TestMethod]
		public void CanCreateRemoteEngineClientFromConnectionString()
		{
			var engine = Engine.For<TestModel>("mode=remote;");
			Assert.IsNotNull(engine);
			Assert.IsNotInstanceOfType(engine, typeof(ILocalEngine<TestModel>));
		}

		[TestMethod]
		public void CanExecuteQuery()
		{
			CanCreateLocalEngineClientFromConfig();
			var count = _engine.Execute(new GetNumberOfCommandsExecutedQuery());
			Assert.AreEqual(0, count);
		}
		[TestMethod]
		public void CanExecuteCommand()
		{
			CanCreateLocalEngineClientFromConfig();
			var count = _engine.Execute(new TestCommandWithResult());
			Assert.AreEqual(1, count);
		}


		[TestMethod]
		public void CanCreateAndUseDispatcherAndMerger()
		{
			// Todo: This test should be a series of smaller tests
			var client = new PartitionClusterClient<TestModel>();
			client.Register<GetNumberOfCommandsExecutedQuery, int>(obj => new[] {1, 2, 3}, ints => ints[1]);

			var dispatcher = client.GetDispatcherFor<GetNumberOfCommandsExecutedQuery>();
			var merger = client.GetMergerFor<GetNumberOfCommandsExecutedQuery, int>();

			var nodeIds = dispatcher.Invoke(new GetNumberOfCommandsExecutedQuery());
			var result = merger.Invoke(nodeIds);

			Assert.AreEqual(2,result);
		}

		/// <summary>
		/// modify this method to switch between sql and file store tests
		/// </summary>
		/// <returns></returns>
		public EngineConfiguration CreateConfig()
		{
			//return CreateSqlConfig();
			return CreateFileConfig();
		}

		private EngineConfiguration CreateFileConfig()
		{
			var config = new EngineConfiguration();
			//Connection string name in app.config file
			config.Location = _path;
			config.SnapshotBehavior = SnapshotBehavior.None;
			config.Synchronization = SynchronizationMode.ReadWrite;
			return config;
		}

		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			if (Directory.Exists(_path)) new DirectoryInfo(_path).Delete(true);
			if (Directory.Exists(_pathForConnectionString)) new DirectoryInfo(_pathForConnectionString).Delete(true);
			Console.WriteLine("Path:" + _path);
			Console.WriteLine("PathForConnectionString:" + _pathForConnectionString);
		}
	}
}
