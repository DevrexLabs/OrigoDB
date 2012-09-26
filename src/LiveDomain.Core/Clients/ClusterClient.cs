using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDomain.Core
{
	public abstract class ClusterClient<M> : IEngine<M> where M : Model
	{
		List<IEngine<M>> _nodes = new List<IEngine<M>>();

		public List<IEngine<M>> Nodes
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