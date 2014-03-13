using System.Collections.Generic;

namespace OrigoDB.Core.Clients.Dispatching
{
	public abstract class QueryDispatchStrategyBase<TModel> : IClusterQueryDispatchStrategy<TModel> where TModel : Model
	{
		protected QueryDispatchStrategyBase(List<RemoteEngineClient<TModel>> nodes)
		{
			Nodes = nodes;
		}
		protected List<RemoteEngineClient<TModel>> Nodes;
		public abstract RemoteEngineClient<TModel> GetDispatcher();
	}
}