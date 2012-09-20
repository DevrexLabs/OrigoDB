using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDomain.Core
{
	public class ClusterClient<M> : IEngine<M>, IDisposable where M : Model, new()
	{
		int _slaveCirkus;
		public RemoteEngineClient<M> MasterNode { get; internal set; }
		public List<RemoteEngineClient<M>> AllNodes { get; internal set; }

		#region IEngine<M> Members

		public T Execute<T>(Query<M, T> query)
		{
			var node = GetQueryNode();
			return node.Execute(query);
		}

		public void Execute(Command<M> command)
		{
			MasterNode.Execute(command);
		}

		public T Execute<T>(CommandWithResult<M, T> command)
		{
			return MasterNode.Execute(command);
		}

		#endregion

		#region Implementation of IDisposable

		bool _disposed;

		public virtual void Dispose()
		{
			if (_disposed) return;
			MasterNode.Dispose();
			foreach (var slave in AllNodes)
			{
				slave.Dispose();
			}
			_disposed = true;
		}

		#endregion

		public void Init(string initHost, int initPort, bool connectionPooling)
		{
			if (AllNodes != null)
				throw new NotSupportedException();

			AllNodes = new List<RemoteEngineClient<M>>();
			var requestContext = RemoteClientConfiguration.RequestContextFactory(initHost, initPort, connectionPooling);
			var initNode = new RemoteEngineClient<M>(requestContext);
			// Request Information
			var response = initNode.SendMessage(new ClusterInfoRequest());
			// Process Master
			if (initHost == response.MasterHost && initPort == response.MasterPort)
			{
				MasterNode = initNode;
			}
			else
			{
				AllNodes.Add(initNode);
				requestContext = RemoteClientConfiguration.RequestContextFactory(response.MasterHost, response.MasterPort,
																				 connectionPooling);
				MasterNode = new RemoteEngineClient<M>(requestContext);
			}
			AllNodes.Add(MasterNode);
			// Process Slaves
			var config = new RemoteClientConfiguration();
			config.ConnectionPooling = connectionPooling;
			foreach (var slave in response.Slaves)
			{
				config.Host = slave.Item1;
				config.Port = slave.Item2;

				if (initHost != config.Host && 
					initPort != config.Port &&
					response.MasterHost != config.Host &&
					response.MasterPort != config.Port)
				{
					AllNodes.Add((RemoteEngineClient<M>)config.GetClient<M>());
				}
			}
		}

		RemoteEngineClient<M> GetQueryNode()
		{
			if (AllNodes == null || AllNodes.Count == 0)
				return MasterNode;

			lock (AllNodes)
			{
				_slaveCirkus++;
				return AllNodes[_slaveCirkus % AllNodes.Count];
			}
		}
	}
}