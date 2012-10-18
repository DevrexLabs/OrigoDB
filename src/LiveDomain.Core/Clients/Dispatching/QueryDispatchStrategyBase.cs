using System.Collections.Generic;

namespace LiveDomain.Core.Clients.Dispatching
{
	public abstract class QueryDispatchStrategyBase<M> : IClusterQueryDispatchStrategy<M> where M : Model
	{
		protected QueryDispatchStrategyBase(List<RemoteEngineClient<M>> nodes)
		{
			Nodes = nodes;
		}
		protected List<RemoteEngineClient<M>> Nodes;
		public abstract RemoteEngineClient<M> GetDispatcher();
	}
}