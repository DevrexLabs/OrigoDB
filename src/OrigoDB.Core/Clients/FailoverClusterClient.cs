using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using OrigoDB.Core.Clients;
using OrigoDB.Core.Clients.Dispatching;

namespace OrigoDB.Core
{
	public class FailoverClusterClient<M> : ClusterClient<M, RemoteEngineClient<M>> where M : Model, new()
	{
		readonly object _lock = new object();
		Guid _clusterId;
		IClusterQueryDispatchStrategy<M> _dispatchStrategy;
		Guid _previousClusterId;

		public FailoverClusterClient(RemoteClientConfiguration configuration)
		{
			Configuration = configuration;
			MasterNode = (RemoteEngineClient<M>)configuration.GetClient<M>();
			// Todo move to config
			_dispatchStrategy = new RoundRobinQueryDispatchStrategy<M>(Nodes);
		}

		public RemoteClientConfiguration Configuration { get; set; }

		public RemoteEngineClient<M> MasterNode
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

		public override T Execute<S, T>(Query<S, T> query)
		{
			RemoteEngineClient<M> node;
			lock (_lock)
			{
				ThrowIfDisconnected();
				node = _dispatchStrategy.GetDispatcher();
			}

			return (T)Execute(node, query);
		}

		bool _throwOnNodeError;
		void RemoveNode(RemoteEngineClient<M> node)
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
			MasterNode = (RemoteEngineClient<M>)Configuration.GetClient<M>();
		}

		public override void Execute<S>(Command<S> command)
		{
			RemoteEngineClient<M> node;
			lock (_lock)
			{
				node = MasterNode;
			}
			Execute(node, command);
		}

		public override T Execute<S, T>(Command<S, T> command)
		{
			RemoteEngineClient<M> node;
			lock (_lock)
			{
				node = MasterNode;
			}
			return (T)Execute(node, command);
		}

		object Execute<T>(RemoteEngineClient<M> node, T objectToExecute)
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

		RemoteEngineClient<M> GetNode(string host, int port)
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
					MasterNode = (RemoteEngineClient<M>)Configuration.GetClient<M>();

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

		RemoteEngineClient<M> CreateNode(string host, int port)
		{
			var nodeConfig = new RemoteClientConfiguration();
			nodeConfig.DedicatedPool = Configuration.DedicatedPool;
			nodeConfig.MaxConnections = Configuration.MaxConnections;
			nodeConfig.Host = host;
			nodeConfig.Port = port;
			var node = (RemoteEngineClient<M>) nodeConfig.GetClient<M>();
			Nodes.Add(node);
			return node;
		}

		public static FailoverClusterClient<M> CreateFromNetwork(RemoteClientConfiguration baseConfiguration)
		{
			var client = new FailoverClusterClient<M>(baseConfiguration);
			var initClient = (RemoteEngineClient<M>)baseConfiguration.GetClient<M>();
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