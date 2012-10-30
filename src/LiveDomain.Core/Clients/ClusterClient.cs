using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDomain.Core
{
	public abstract class ClusterClient<M> : ClusterClient<M, IEngine<M>> where M : Model
	{
	}

	public abstract class ClusterClient<M,N> : IEngine<M> where M : Model
	{
		List<N> _nodes = new List<N>();

		public List<N> Nodes
		{
			get { return _nodes; }
		}

		#region Implementation of IEngine<M>

		public abstract T Execute<T>(Query<M, T> query);
		public abstract void Execute(Command<M> command);
		public abstract T Execute<T>(CommandWithResult<M, T> command);

		#endregion
	}
}