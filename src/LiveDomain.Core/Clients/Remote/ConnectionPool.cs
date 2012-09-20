using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace LiveDomain.Core
{
	internal static class ConnectionPools
	{
		//Keyed by Host:Port
		private static Dictionary<string, ConnectionPool> _connectionPools = new Dictionary<string, ConnectionPool>();

		public static ConnectionPool GetPoolFor(string host, int port, int maxPoolSize)
		{
			lock (_connectionPools)
			{
				ConnectionPool pool;
				var key = string.Format("{0}:{1}", host, port);
				if (!_connectionPools.TryGetValue(key, out pool))
				{
					_connectionPools[key] = new ConnectionPool(() => new TcpClient(host, port), maxPoolSize);
				}
				return pool;
			}
		}
	}
}