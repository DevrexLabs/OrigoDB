using System;
using System.Collections.Generic;

namespace OrigoDB.Core.Clients.Dispatching
{
	public class RandomQueryDispatchStrategy<M> : QueryDispatchStrategyBase<M> where M : Model
	{

		readonly Random _random = new Random();

		public RandomQueryDispatchStrategy(List<RemoteEngineClient<M>> clients) : base(clients)
		{ }

		public override RemoteEngineClient<M> GetDispatcher()
		{
			if(Nodes.Count == 1) return Nodes[0];

			var clientId = _random.Next(1, Nodes.Count);
			return Nodes[clientId];
		}
	}
}