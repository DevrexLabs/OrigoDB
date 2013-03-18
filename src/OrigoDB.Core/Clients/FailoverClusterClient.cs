using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using OrigoDB.Core.Clients.Dispatching;

namespace OrigoDB.Core
{
	public class FailoverClusterClient<M> : ClusterClient<M, RemoteEngineClient<M>> where M : Model, new()
	{
		readonly Random _randomizer = new Random();
		Guid _clusterId;
		IClusterQueryDispatchStrategy<M> _dispatchStrategy;


		public FailoverClusterClient(RemoteClientConfiguration configuration)
		{
			Configuration = configuration;
			MasterNode = (RemoteEngineClient<M>)configuration.GetClient<M>();
			// Todo move to config
			_dispatchStrategy = new RandomQueryDispatchStrategy<M>(Nodes);
		}

		public RemoteClientConfiguration Configuration { get; set; }
		public RemoteEngineClient<M> MasterNode
		{
			get
			{
				return Nodes[0];
			}
			private set
			{
				if (Nodes.Count == 0)
					Nodes.Add(value);
				else
					Nodes.Insert(0, value);
			}
		}

		public override T Execute<S, T>(Query<S, T> query)
		{

			RemoteEngineClient<M> node;
			lock (this)
			{
				node = _dispatchStrategy.GetDispatcher();
			}

			T result;
			try
			{
			 result = (T)Execute(node, query);
			}
			catch(NotSupportedException)
			{
				lock(this)
				{
					if(Nodes.Contains(node))
						Nodes.Remove(node);
				}
				return Execute(query);
			}

			return result;
		}

		public override void Execute<S>(Command<S> command)
		{
			RemoteEngineClient<M> node;
			lock (this)
			{
				node = MasterNode;
			}
			Execute(node, command);
		}

		public override T Execute<S, T>(CommandWithResult<S, T> command)
		{
			RemoteEngineClient<M> node;
			lock (this)
			{
				node = MasterNode;
			}
			return (T)Execute(node, command);
		}

		object Execute<T>(RemoteEngineClient<M> node, T objectToExecute)
		{
			object result = null;
			try
			{
				var request = new ClusterExecuteRequest(_clusterId, objectToExecute);
				result = node.SendAndRecieve(request);
			}
			catch (Exception e)
			{
				if (e is SocketException || e is IOException)
				{
					lock (this)
					{
						if(Nodes.Contains(node))
							Nodes.Remove(node);
						node = MasterNode;
					}
					return Execute(node, objectToExecute);
				}
				throw;
			}
			
			if (result is ClusterExecuteResponse)
			{
				var msg = result as ClusterExecuteResponse;
				if (msg.ClusterUpdated)
					UpdateClusterInformation(msg.ClusterInfo);
				return msg.Payload;
			}


			throw new NotSupportedException("Format of returned data is unexpected.");
		}

		Guid _previousClusterId;
		void UpdateClusterInformation(ClusterInfo clusterInfo)
		{
			lock (this)
			{
				if (clusterInfo.Id == Guid.Empty || _clusterId == clusterInfo.Id || _previousClusterId == clusterInfo.Id) return;
				_previousClusterId = _clusterId; 

				Configuration.Host = clusterInfo.MasterHost;
				Configuration.Port = clusterInfo.MasterPort;
				Nodes.Clear();
				MasterNode = (RemoteEngineClient<M>)Configuration.GetClient<M>();

				foreach (var hostAndPort in clusterInfo.Slaves)
				{
					var nodeConfig = new RemoteClientConfiguration();
					nodeConfig.DedicatedPool = Configuration.DedicatedPool;
					nodeConfig.MaxConnections = Configuration.MaxConnections;
					nodeConfig.Host = hostAndPort.Key;
					nodeConfig.Port = hostAndPort.Value;
					Nodes.Add((RemoteEngineClient<M>)nodeConfig.GetClient<M>());
				}

				_clusterId = clusterInfo.Id;
			}
		}

		public static FailoverClusterClient<M> CreateFromNetwork(RemoteClientConfiguration baseConfiguration)
		{
			var client = new FailoverClusterClient<M>(baseConfiguration);
			var initClient = (RemoteEngineClient<M>)baseConfiguration.GetClient<M>();
			var response = initClient.SendAndRecieve<ClusterInfoResponse>(new ClusterInfoRequest());
			client.UpdateClusterInformation(response.ClusterInfo);
			return client;
		}
	}
}