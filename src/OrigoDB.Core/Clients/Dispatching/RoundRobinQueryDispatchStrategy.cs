using System.Collections.Generic;

namespace OrigoDB.Core.Clients.Dispatching
{
	public class RoundRobinQueryDispatchStrategy<M> : QueryDispatchStrategyBase<M> where M : Model
	{
		readonly bool _includeMaster;
		int _counter;

		public RoundRobinQueryDispatchStrategy(List<RemoteEngineClient<M>> clients, bool includeMaster = true) : base(clients)
		{
			_includeMaster = includeMaster;
			_counter = 1;
		}

		public override RemoteEngineClient<M> GetDispatcher()
		{
			if (Nodes.Count == 0)
				return null;

			if (_counter >= Nodes.Count)
				_counter = _includeMaster ? 0 : 1;
				
			var node = Nodes[_counter < Nodes.Count ? _counter : 0];
			++_counter;
			
			return node;
		}
	}
}