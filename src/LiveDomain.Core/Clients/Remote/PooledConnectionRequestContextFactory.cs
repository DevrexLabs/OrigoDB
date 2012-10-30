using System.Net.Sockets;
using LiveDomain.Core;

namespace LiveDomain.Core
{
	public class PooledConnectionRequestContextFactory : IRequestContextFactory
	{
		readonly ConnectionPool _pool;

		public PooledConnectionRequestContextFactory(ConnectionPool pool)
		{
			_pool = pool;
		}

		public RequestContext GetContext()
		{
			var tcpClient = _pool.AquireResource();
			var disposable = new Disposable<RemoteConnection>(tcpClient, () => _pool.Release(tcpClient));
			return new RequestContext(disposable);
		}

		public void Dispose()
		{

		}

	}
}