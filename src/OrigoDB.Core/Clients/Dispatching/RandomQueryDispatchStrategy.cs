using System;
using System.Collections.Generic;

namespace OrigoDB.Core.Clients.Dispatching
{
	public class RandomQueryDispatchStrategy<TModel> : QueryDispatchStrategyBase<TModel> where TModel : Model
	{
		readonly bool _includeMaster;
		readonly Random _random = new Random();

		public RandomQueryDispatchStrategy(List<RemoteEngineClient<TModel>> clients, bool includeMaster = true) : base(clients)
		{
			_includeMaster = includeMaster;
		}

		public override RemoteEngineClient<TModel> GetDispatcher()
		{
			if(Nodes.Count == 0) return null;
			if(Nodes.Count == 1) return Nodes[0];

			var clientId = _random.Next(1, Nodes.Count);
			return Nodes[clientId];
		}
	}
}