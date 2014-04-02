using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using OrigoDB.Core.Clients;
using OrigoDB.Core.Clients.Dispatching;

namespace OrigoDB.Core
{
	public class FailoverClusterClient<TModel> : ClusterClient<TModel, RemoteEngineClient<TModel>> where TModel : Model, new()
	{
		readonly object _lock = new object();
		Guid _clusterId;
		IClusterQueryDispatchStrategy<TModel> _dispatchStrategy;
		Guid _previousClusterId;

		public FailoverClusterClient(RemoteClientConfiguration configuration)
		{
			Configuration = configuration;
			MasterNode = (RemoteEngineClient<TModel>)configuration.GetClient<TModel>();
			// Todo move to config
			_dispatchStrategy = new RoundRobinQueryDispatchStrategy<TModel>(Nodes);
		}

		public RemoteClientConfiguration Configuration { get; set; }

		public RemoteEngineClient<TModel> MasterNode
		{
			get
			{
				ThrowIfDisconnected();
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

		public override TResult Execute<TResult>(Query<TModel, TResult> query)
		{
			RemoteEngineClient<TModel> node;
			lock (_lock)
			{
				ThrowIfDisconnected();
				node = _dispatchStrategy.GetDispatcher();
			}

			return (TResult)Execute(node, query);
		}

		bool _throwOnNodeError;
		void RemoveNode(RemoteEngineClient<TModel> node)
		{
			var nodeIndex = Nodes.IndexOf(node);

			if (nodeIndex >= 0)
			{
				Nodes.Remove(node);
			}

			if ((nodeIndex == 0 || Nodes.Count == 0))
			{
				if (_throwOnNodeError)
					throw new NotSupportedException("Lost connection to master.");

				_throwOnNodeError = true;
				ResetConnection();
			}
		}

		public void ResetConnection()
		{
			_clusterId = Guid.Empty;
			_previousClusterId = Guid.Empty;
			MasterNode = (RemoteEngineClient<TModel>)Configuration.GetClient<TModel>();
		}

		public override void Execute(Command<TModel> command)
		{
			RemoteEngineClient<TModel> node;
			lock (_lock)
			{
				node = MasterNode;
			}
			Execute(node, command);
		}

		public override TResult Execute<TResult>(Command<TModel, TResult> command)
		{
			RemoteEngineClient<TModel> node;
			lock (_lock)
			{
				node = MasterNode;
			}
			return (TResult)Execute(node, command);
		}

		object Execute<TMessage>(RemoteEngineClient<TModel> node, TMessage objectToExecute)
		{
			object result = null;
			var request = new ClusterExecuteRequest(_clusterId, objectToExecute);
			try
			{
				result = node.SendAndRecieve(request);
			}
			catch (WrongNodeException e)
			{
				lock (_lock)
				{
					node = GetNode(e.Host, e.Port);
				}
				return Execute(node, objectToExecute);
			}
			catch (Exception e)
			{
				if (e is SocketException || e is IOException)
				{
					lock (_lock)
					{
						RemoveNode(node);
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

		RemoteEngineClient<TModel> GetNode(string host, int port)
		{
			var node = Nodes.FirstOrDefault(n => n.Host.Equals(host, StringComparison.OrdinalIgnoreCase) && n.Port == port);

			if (node == null)
				node = CreateNode(host, port);

			return node;
		}

		void UpdateClusterInformation(ClusterInfo clusterInfo)
		{
			lock (_lock)
			{

				if (clusterInfo.Id == Guid.Empty || _clusterId == clusterInfo.Id || _previousClusterId == clusterInfo.Id) return;
				_previousClusterId = _clusterId;

				_throwOnNodeError = false;

				// Lock configuration since it's shared with all clients created by GetClient on the configuration.
				lock (Configuration)
				{
					Configuration.Host = clusterInfo.MasterHost;
					Configuration.Port = clusterInfo.MasterPort;
					Nodes.Clear();
					MasterNode = (RemoteEngineClient<TModel>)Configuration.GetClient<TModel>();

					foreach (var hostAndPort in clusterInfo.Slaves)
					{
						var host = hostAndPort.Key;
						var port = hostAndPort.Value;
						CreateNode(host, port);
					}
				}
				_clusterId = clusterInfo.Id;
			}
		}

		RemoteEngineClient<TModel> CreateNode(string host, int port)
		{
			var nodeConfig = new RemoteClientConfiguration();
			nodeConfig.DedicatedPool = Configuration.DedicatedPool;
			nodeConfig.MaxConnections = Configuration.MaxConnections;
			nodeConfig.Host = host;
			nodeConfig.Port = port;
			var node = (RemoteEngineClient<TModel>) nodeConfig.GetClient<TModel>();
			Nodes.Add(node);
			return node;
		}

		public static FailoverClusterClient<TModel> CreateFromNetwork(RemoteClientConfiguration baseConfiguration)
		{
			var client = new FailoverClusterClient<TModel>(baseConfiguration);
			var initClient = (RemoteEngineClient<TModel>)baseConfiguration.GetClient<TModel>();
			var response = initClient.SendAndRecieve<ClusterInfoResponse>(new ClusterInfoRequest());
			client.UpdateClusterInformation(response.ClusterInfo);
			return client;
		}

		void ThrowIfDisconnected()
		{
			if (Nodes.Count == 0)
				throw new NotSupportedException("This client is disconnected.");
		}
	}
}