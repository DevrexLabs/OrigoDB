using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

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
