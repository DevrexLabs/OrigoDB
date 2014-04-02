namespace OrigoDB.Core.Clients.Dispatching
{
	public class DedicatedQueryDispatchStrategy<TModel> : IClusterQueryDispatchStrategy<TModel> where TModel : Model
	{
		readonly RemoteEngineClient<TModel> _client;

		public DedicatedQueryDispatchStrategy(RemoteEngineClient<TModel> client)
		{
			_client = client;
		}

		public RemoteEngineClient<TModel> GetDispatcher()
		{
			return _client;
		}
	}
}