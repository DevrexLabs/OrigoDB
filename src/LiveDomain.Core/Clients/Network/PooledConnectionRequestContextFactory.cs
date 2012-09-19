using System.Net.Sockets;
using LiveDomain.Core;

namespace LiveDomain.Core
{
	public class PooledConnectionRequestContextFactory : IRequestContextFactory
	{
		readonly ConnectionPool _pool;

		public PooledConnectionRequestContextFactory(string host,int port,int maxPoolConnections)
		{
			_pool = ConnectionPools.GetPoolFor(host, port, maxPoolConnections);
    	}

		public RequestContext GetContext()
		{
			var tcpClient = _pool.AquireResource();
			var disposable = new Disposable<TcpClient>(tcpClient, () => _pool.Release(tcpClient));
			return new RequestContext(disposable);
		}

		public void Dispose()
		{

		}

	}
}