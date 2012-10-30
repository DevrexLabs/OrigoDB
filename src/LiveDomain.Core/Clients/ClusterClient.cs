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

		public abstract T Execute<S,T>(Query<S, T> query) where S : Model;
		public abstract void Execute<S>(Command<S> command) where S : Model;
		public abstract T Execute<S,T>(CommandWithResult<S, T> command) where S : Model;

		#endregion
	}
}