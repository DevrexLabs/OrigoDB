namespace OrigoDB.Core.Clients.Dispatching
{
	public interface IClusterQueryDispatchStrategy<M> where M : Model
	{
		RemoteEngineClient<M> GetDispatcher();
	}
}
