using System;

namespace OrigoDB.Core
{
	public class ConnectionPool : ResourcePool<RemoteConnection>
	{
		public string Host { get; set; }
		public int Port { get; set; }

		internal ConnectionPool(Func<RemoteConnection> constructor, int maxSize = 100)
			: base(constructor, maxSize)
		{
		}

		internal ConnectionPool(Func<RemoteConnection> constructor, int maxConnections, string host, int port) : base(constructor,maxConnections)
		{
			Host = host;
			Port = port;
		}
	}
}
