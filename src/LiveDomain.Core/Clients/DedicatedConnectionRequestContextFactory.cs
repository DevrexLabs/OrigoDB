using System.Net.Sockets;
using LiveDomain.Core;

namespace LiveDomain.Core
{
	public class DedicatedConnectionRequestContextFactory : IRequestContextFactory
	{
		readonly TcpClient _client;
		bool _disposed;

		public DedicatedConnectionRequestContextFactory(TcpClient client)
		{
			_client = client;
		}

		public RequestContext GetContext()
		{
			var disposable = new Disposable<TcpClient>(_client);
			return new RequestContext(disposable);
		}
		
		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;
			_client.Close();
		}
	}
}