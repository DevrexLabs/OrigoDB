using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveDomain.Core
{
	public class RemoteNode<T>
	{
		public int NodeId { get; set; }
		public T Client { get; set; }
	}
	
	public class ClusterClient<M> : IEngine<M>, IDisposable where M : Model
	{
		public RemoteEngineClient<M> MasterNode { get; internal set; }
		public List<RemoteEngineClient<M>> SlaveNodes { get; internal set; }
		public int Id { get; private set; }

		public ClusterClient()
		{
			SlaveNodes = new List<RemoteEngineClient<M>>();
		}

		public bool Init(RemoteEngineClient<M> initialClient)
		{
			// Todo : Ask the initial client about the cluster (id, master/slave information etc.)
			var response = initialClient.SendMessage(new ClusterInfoRequest());
			Id = response.Id;
			
			foreach (var slave in response.Slaves)
			{
				
			}
			return true;
		}

		public T Execute<T>(Query<M, T> query)
		{
			throw new NotImplementedException();
		}

		public void Execute(Command<M> command)
		{
			throw new NotImplementedException();
		}

		public T Execute<T>(CommandWithResult<M, T> command)
		{
			throw new NotImplementedException();
		}
		
		#region Implementation of IDisposable

		public virtual void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}