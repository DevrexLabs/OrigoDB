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
			//var client = new PartitionClient<TestModel>();
			//client.Register<GetNumberOfCommandsExecutedQuery, int>(obj => new[] {1, 2, 3}, ints => ints[1]);

			//var dispatcher = client.GetDispatcherFor<GetNumberOfCommandsExecutedQuery>();
			//var merger = client.GetMergerFor<int>();

			//var nodeIds = dispatcher.Invoke(new GetNumberOfCommandsExecutedQuery());
			//var result = merger.Invoke(nodeIds);

			//Assert.AreEqual(2,result);
		}

		[TestMethod]
		public void LocalEngineClientReusesEngineReferences()
		{
			var localConfig1 = new LocalClientConfiguration(CreateConfig());
			var localConfig2 = new LocalClientConfiguration(CreateConfig());
			var engine1 = (LocalEngineClient<TestModel>)localConfig1.GetClient<TestModel>();
			var engine2 = (LocalEngineClient<TestModel>)localConfig2.GetClient<TestModel>();

			Assert.AreSame(engine1.Engine,engine2.Engine);
		}

		[TestMethod]
		public void PartitionClient()
		{
			Engines.CloseAll();
			DeleteOldTestdata();

			var client = new PartitionClient<TestModel>();
			client[0] = Engine.For<TestModel>("mode=embedded;location=" + _path);
			client[1] = Engine.For<TestModel>("mode=embedded;location=" + _pathForConnectionString);

			// Register partition mapping
			client.Register<GetNumberOfCommandsExecutedQuery,int>(query => new[] {0},values => values[0]);
			client.Register<TestCommandWithoutResult>(query => new[] { 1 });
			client.Register<TestCommandWithResult, int>(query => new[] { 0,1 }, values => values[0] + values[1]);
			
			var response = client.Execute(new TestCommandWithResult());
			
			Assert.AreEqual(2,response);

			// Check model values
			var response1 = client[0].Execute(new GetNumberOfCommandsExecutedQuery());
			var response2 = client[1].Execute(new GetNumberOfCommandsExecutedQuery());

			Assert.AreEqual(1, response1);
			Assert.AreEqual(1, response2);
			
			client.Execute(new TestCommandWithoutResult());
			response1 = client[0].Execute(new GetNumberOfCommandsExecutedQuery());
			response2 = client[1].Execute(new GetNumberOfCommandsExecutedQuery());

			Assert.AreEqual(1,response1);
			Assert.AreEqual(2, response2);
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
			DeleteOldTestdata();
		}

		static void DeleteOldTestdata()
		{
			if (Directory.Exists(_path)) new DirectoryInfo(_path).Delete(true);
			if (Directory.Exists(_pathForConnectionString)) new DirectoryInfo(_pathForConnectionString).Delete(true);
			Console.WriteLine("Path:" + _path);
			Console.WriteLine("PathForConnectionString:" + _pathForConnectionString);
		}
	}
}
