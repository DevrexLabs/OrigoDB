namespace OrigoDB.Core.Clients.Dispatching
{
	public interface IClusterQueryDispatchStrategy<TModel> where TModel : Model
	{
		RemoteEngineClient<TModel> GetDispatcher();
	}
}
