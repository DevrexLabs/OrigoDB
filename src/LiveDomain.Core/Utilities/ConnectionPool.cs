using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LiveDomain.Core
{
	public class ConnectionPool : ResourcePool<TcpClient>
	{
		internal ConnectionPool(Func<TcpClient> constructor, int maxSize = 100)
			: base(constructor, maxSize)
		{
		}
	}
}
