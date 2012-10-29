using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LiveDomain.Core
{
	public class ConnectionPool : ResourcePool<RemoteConnection>
	{
		internal ConnectionPool(Func<RemoteConnection> constructor, int maxSize = 100)
			: base(constructor, maxSize)
		{
		}
	}
}
