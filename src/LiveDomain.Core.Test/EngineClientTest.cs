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
		static string _path = "c:\\db\\engineClientDb";
		static string _pathForConnectionString = "c:\\db\\engineClientDb2";

		[TestMethod]
		public void CanCreateLocalEngineClientFromConnectionString()
		{
			ResetData();
			var engine = Engine.For<TestModel>("mode=embedded;location=" + _pathForConnectionString);
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine, typeof(ILocalEngine<TestModel>));
		}

		[TestMethod]
		public void CanCreateLocalEngineClientFromModel()
		{
			ResetData();
			var engine = Engine.For<TestModel>();
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine, typeof(LocalEngineClient<TestModel>));
		}

		[TestMethod]
		public void CanCreateLocalEngineClientFromConfig()
		{
			ResetData();
			var engine = Engine.For<TestModel>(CreateConfig());
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine, typeof(LocalEngineClient<TestModel>));
		}

		[TestMethod]
		public void CanCreateRemoteEngineClientFromConnectionString()
		{
			ResetData();
			var engine = Engine.For<TestModel>("mode=remote;");
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine,typeof(RemoteEngineClient<TestModel>));
		}

		[TestMethod]
		public void CanExecuteQuery()
		{
			ResetData();
			var engine = Engine.For<TestModel>(CreateConfig());
			var count = engine.Execute(new GetNumberOfCommandsExecutedQuery());
			Assert.AreEqual(0, count);
		}
		[TestMethod]
		public void CanExecuteCommand()
		{
			ResetData();
			var engine = Engine.For<TestModel>(CreateConfig());
			var count = engine.Execute(new TestCommandWithResult());
			Assert.AreEqual(1, count);
		}

		[TestMethod]
		public void LocalEngineClientReusesEngineReferences()
		{
			ResetData(); // Close all engines and delete model data.
			var localConfig1 = new LocalClientConfiguration(CreateConfig());
			var localConfig2 = new LocalClientConfiguration(CreateConfig());
			var client1 = (LocalEngineClient<TestModel>)localConfig1.GetClient<TestModel>();
			var client2 = (LocalEngineClient<TestModel>)localConfig2.GetClient<TestModel>();
			
			Assert.AreSame(client1.Engine,client2.Engine);
		}

		[TestMethod]
		public void PartitionClient()
		{
			ResetData(); // Close all engines and delete model data.
			
			var client = new PartitionClusterClient<TestModel>();
			var engine1 = Engine.For<TestModel>("mode=embedded;location=" + _path);
			var engine2 = Engine.For<TestModel>("mode=embedded;location=" + _pathForConnectionString);

			client.Nodes.Add(engine1);
			client.Nodes.Add(engine2);

			// Set dispatchers and mergers for types
			client.SetDispatcherFor<TestCommandWithoutResult>(command => 1);
			client.SetMergerFor<int>(i => i.Sum());
			
			var response = client.Execute(new TestCommandWithResult());
			Assert.AreEqual(2,response);
			
			// Check model values
			var response1 = client.Nodes[0].Execute(new GetNumberOfCommandsExecutedQuery());
			var response2 = client.Nodes[1].Execute(new GetNumberOfCommandsExecutedQuery());

			Assert.AreEqual(1, response1);
			Assert.AreEqual(1, response2);
			
			client.Execute(new TestCommandWithoutResult());

			// Check model values again
			response1 = client.Nodes[0].Execute(new GetNumberOfCommandsExecutedQuery());
			response2 = client.Nodes[1].Execute(new GetNumberOfCommandsExecutedQuery());

			Assert.AreEqual(1,response1);
			Assert.AreEqual(2, response2);
		}

		

		/// <summary>
		/// modify this method to switch between sql and file store tests
		/// </summary>
		/// <returns></returns>
		public EngineConfiguration CreateConfig()
		{
		
			var config = new EngineConfiguration();
			//Connection string name in app.config file
			config.Location = _path;
			config.SnapshotBehavior = SnapshotBehavior.None;
			config.Synchronization = SynchronizationMode.ReadWrite;
			return config;
		}

		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//    ResetData();
		//}

		static void ResetData()
		{
			Config.Engines.CloseAll();
			if (Directory.Exists(_path)) new DirectoryInfo(_path).Delete(true);
			if (Directory.Exists(_pathForConnectionString)) new DirectoryInfo(_pathForConnectionString).Delete(true);
			//Console.WriteLine("Path:" + _path);
			//Console.WriteLine("PathForConnectionString:" + _pathForConnectionString);
		}
	}
}
