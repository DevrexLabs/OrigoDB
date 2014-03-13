using System.Collections.Generic;

namespace OrigoDB.Core
{
	public abstract class ClusterClient<TModel> : ClusterClient<TModel, IEngine<TModel>> where TModel : Model
	{
	}

	public abstract class ClusterClient<TModel,TEngine> : IEngine<TModel> where TModel : Model
	{
		readonly List<TEngine> _nodes = new List<TEngine>();

		public List<TEngine> Nodes
		{
			get { return _nodes; }
		}

	    public abstract TResult Execute<TResult>(Query<TModel, TResult> query);
	    public abstract void Execute(Command<TModel> command);

	    public abstract TResult Execute<TResult>(Command<TModel, TResult> command);
	}
}