using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Clients
{
	public class WrongNodeException : Exception
	{
		public WrongNodeException(string host, int port)
		{
			Host = host;
			Port = port;
		}

		public string Host { get; set; }
		public int Port { get; set; }
	}
}
