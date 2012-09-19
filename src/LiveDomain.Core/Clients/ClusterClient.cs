using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDomain.Core
{
	public class ClusterClient<M> : IEngine<M>, IDisposable where M : Model, new()
	{
		public RemoteEngineClient<M> MasterNode { get; internal set; }
		public List<RemoteEngineClient<M>> SlaveNodes { get; internal set; }
		
		public void Init(string initHost, int initPort, bool connectionPooling)
		{
			if(SlaveNodes != null)
				throw new NotSupportedException();
			
			var requestContext = RemoteClientConfiguration.RequestContextFactory(initHost, initPort,connectionPooling);
			var initNode = new RemoteEngineClient<M>(requestContext);
			// Request Information
			var response = initNode.SendMessage(new ClusterInfoRequest());
			// Process Master
			if (initHost == response.MasterHost && initPort == response.MasterPort)
				MasterNode = initNode;
			else
			{
				requestContext = RemoteClientConfiguration.RequestContextFactory(response.MasterHost, response.MasterPort, connectionPooling);
				MasterNode = new RemoteEngineClient<M>(requestContext);
			}
			// Process Slaves
			SlaveNodes = new List<RemoteEngineClient<M>>();
			var config = new RemoteClientConfiguration();
			config.ConnectionPooling = connectionPooling;
			foreach (var slave in response.Slaves)
			{
				config.Host = slave.Item1;
				config.Port = slave.Item2;
				
				if (initHost == config.Host && initPort == config.Port && !SlaveNodes.Contains(initNode))
				{
					SlaveNodes.Add(initNode);
					continue;
				}
				SlaveNodes.Add((RemoteEngineClient<M>) config.GetClient<M>());
			}
		}

		

		int _slaveCirkus;
		public T Execute<T>(Query<M, T> query)
		{
			RemoteEngineClient<M> node;
			lock (SlaveNodes)
			{
				var id = Interlocked.Increment(ref _slaveCirkus);
				node = SlaveNodes[id % SlaveNodes.Count];	
			}
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

		#region Implementation of IDisposable

		bool _disposed;
		public virtual void Dispose()
		{
			if(_disposed) return;
			MasterNode.Dispose();
			foreach (var slave in SlaveNodes)
			{
				slave.Dispose();
			}
			_disposed = true;
		}

		#endregion
	}
}