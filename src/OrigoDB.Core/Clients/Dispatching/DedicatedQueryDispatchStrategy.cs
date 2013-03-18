namespace OrigoDB.Core.Clients.Dispatching
{
	public class DedicatedQueryDispatchStrategy<M> : IClusterQueryDispatchStrategy<M> where M : Model
	{
		readonly RemoteEngineClient<M> _client;

		public DedicatedQueryDispatchStrategy(RemoteEngineClient<M> client)
		{
			_client = client;
		}

		public RemoteEngineClient<M> GetDispatcher()
		{
			return _client;
		}
	}
}