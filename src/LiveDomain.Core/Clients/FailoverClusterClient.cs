using System;
using System.Threading;
using LiveDomain.Core.Clients.Dispatching;

namespace LiveDomain.Core
{
	public class FailoverClusterClient<M> : ClusterClient<M, RemoteEngineClient<M>> where M : Model, new()
	{
		readonly Random _randomizer = new Random();
		Guid _clusterId;
		IClusterQueryDispatchStrategy<M> _dispatchStrategy; 


		public FailoverClusterClient(RemoteClientConfiguration configuration)
		{
			Configuration = configuration;
			MasterNode = (RemoteEngineClient<M>) configuration.GetClient<M>();
			// Todo move to config
			_dispatchStrategy = new RandomQueryDispatchStrategy<M>(Nodes);
		}
		
		public RemoteClientConfiguration Configuration { get; set; }
		public RemoteEngineClient<M> MasterNode
		{
			get { return Nodes[0]; }
			private set
			{
				if (Nodes.Count == 0)
					Nodes.Add(value);
				else
					Nodes.Insert(0, value);
			}
		}

		public override T Execute<T>(Query<M, T> query)
		{
			return (T) Execute(_dispatchStrategy.GetDispatcher(), query);
		}

		public override void Execute(Command<M> command)
		{
			Execute(MasterNode, command);
		}

		public override T Execute<T>(CommandWithResult<M, T> command)
		{
			return (T) Execute(MasterNode, command);
		}

		object Execute<T>(RemoteEngineClient<M> node, T objectToExecute)
		{
			var request = new ClusterExecuteRequest(_clusterId, objectToExecute);
			var result = node.SendMessage(request);
			if (result is ClusterExecuteResponse)
			{
				var msg = result as ClusterExecuteResponse;
				if (msg.ClusterUpdated)
					UpdateClusterInformation(msg.ClusterInfo);
				return msg.Payload;
			}

			throw new NotSupportedException("Format of returned data is unexpected.");
		}

		void UpdateClusterInformation(ClusterInfo clusterInfo)
		{
			lock (this)
			{
				if (_clusterId == clusterInfo.Id) return;

				var nodeConfig = new RemoteClientConfiguration();
				nodeConfig.DedicatedPool = Configuration.DedicatedPool;
				nodeConfig.MaxConnections = Configuration.MaxConnections;
				nodeConfig.Host = clusterInfo.MasterHost;
				nodeConfig.Port = clusterInfo.MasterPort;

				MasterNode = (RemoteEngineClient<M>) nodeConfig.GetClient<M>();
				Nodes.Clear();
				foreach (var hostAndPort in clusterInfo.Slaves)
				{
					nodeConfig.Host = hostAndPort.Key;
					nodeConfig.Port = hostAndPort.Value;
					Nodes.Add((RemoteEngineClient<M>) nodeConfig.GetClient<M>());
				}

				_clusterId = clusterInfo.Id;
			}
		}

		public static FailoverClusterClient<M> CreateFromNetwork(RemoteClientConfiguration baseConfiguration)
		{
			var client = new FailoverClusterClient<M>(baseConfiguration);
			var initClient = (RemoteEngineClient<M>) baseConfiguration.GetClient<M>();
			var response = initClient.SendMessage<ClusterInfoResponse>(new ClusterInfoRequest());
			client.UpdateClusterInformation(response.ClusterInfo);
			return client;
		}
	}
}