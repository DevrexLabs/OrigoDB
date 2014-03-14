using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrigoDB.Core.Test
{
	[TestClass]
	public class EngineClientTest : EngineTestBase
	{
		[TestMethod]
		public void CanCreateLocalEngineClientFromConnectionString()
		{
			var engine = Engine.For<TestModel>("mode=embedded;location=" + Guid.NewGuid().ToString());
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine, typeof(LocalEngineClient<TestModel>));
		}

		[TestMethod]
		public void CanCreateLocalEngineClientFromModel()
		{
			var engine = Engine.For<TestModel>();
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine, typeof(LocalEngineClient<TestModel>));
		}

		[TestMethod]
		public void CanCreateLocalEngineClientFromConfig()
		{
			var engine = Engine.For<TestModel>(CreateConfig());
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine, typeof(LocalEngineClient<TestModel>));
		}

		[TestMethod]
		public void CanCreateRemoteEngineClientFromConnectionString()
		{
			var engine = Engine.For<TestModel>("mode=remote;");
			Assert.IsNotNull(engine);
			Assert.IsInstanceOfType(engine,typeof(FailoverClusterClient<TestModel>));
		}

		[TestMethod]
		public void CanExecuteQuery()
		{
			var engine = Engine.For<TestModel>(CreateConfig());
			var count = engine.Execute(new GetNumberOfCommandsExecutedQuery());
			Assert.AreEqual(0, count);
		}

		[TestMethod]
		public void CanExecuteCommand()
		{
			var engine = Engine.For<TestModel>(CreateConfig());
			var count = engine.Execute(new TestCommandWithResult());
			Assert.AreEqual(1, count);
		}

		[TestMethod]
		public void LocalEngineClientReusesEngineReferences()
		{
			var config = CreateConfig();
			var localConfig1 = new LocalClientConfiguration(config);
			var localConfig2 = new LocalClientConfiguration(config);
			var client1 = (LocalEngineClient<TestModel>)localConfig1.GetClient<TestModel>();
			var client2 = (LocalEngineClient<TestModel>)localConfig2.GetClient<TestModel>();
			Assert.AreSame(client1.Engine,client2.Engine);
		}

		[TestMethod]
		public void PartitionClient()
		{
			
			var client = new PartitionClient<TestModel>();
			var engine1 = Engine.For<TestModel>("mode=embedded;location=" + Guid.NewGuid());
			var engine2 = Engine.For<TestModel>("mode=embedded;location=" + Guid.NewGuid());

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

		
		[TestCleanup()]
		public void TestCleanup()
		{
			Config.Engines.CloseAll();
		}
	}
}
