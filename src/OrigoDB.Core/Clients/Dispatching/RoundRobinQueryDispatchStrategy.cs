using System.Collections.Generic;

namespace OrigoDB.Core.Clients.Dispatching
{
	public class RoundRobinQueryDispatchStrategy<M> : QueryDispatchStrategyBase<M> where M : Model
	{
		int _counter;

		public RoundRobinQueryDispatchStrategy(List<RemoteEngineClient<M>> clients) : base(clients) {}

		public override RemoteEngineClient<M> GetDispatcher()
		{
			var node = Nodes[_counter];
			_counter++;
			if(_counter == Nodes.Count)
				_counter = 0;
			return node;
		}
	}
}