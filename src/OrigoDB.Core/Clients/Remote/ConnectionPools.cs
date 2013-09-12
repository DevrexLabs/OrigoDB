using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
	internal static class ConnectionPools
	{
		private static Dictionary<string, ConnectionPool> _connectionPools = new Dictionary<string, ConnectionPool>();

		public static ConnectionPool PoolFor(RemoteClientConfiguration configuration)
		{
			lock (_connectionPools)
			{
				ConnectionPool pool;
				var key = configuration.ToString();
				if(configuration.DedicatedPool) key +=  Guid.NewGuid();

				if (!_connectionPools.TryGetValue(key, out pool))
				{
					var host = configuration.Host;
					var port = configuration.Port;
					var maxConnections = configuration.MaxConnections;
					pool = new ConnectionPool(() => new RemoteConnection(host, port), maxConnections,host,port);
					_connectionPools[key] = pool;
				}
				return pool;
			}
		}
	}
}