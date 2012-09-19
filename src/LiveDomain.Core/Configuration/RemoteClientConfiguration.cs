using System;
using System.Net.Sockets;
using LiveDomain.Core;

namespace LiveDomain.Core
{
    public class RemoteClientConfiguration : ClientConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool ConnectionPooling { get; set; }
        //public int MaxPoolConnections { get; set; } Todo: This will be supported later!

        public RemoteClientConfiguration()
        {
            Host = "localhost";
            Port = 9292;
            ConnectionPooling = true;
        }

	    #region Overrides of ClientConfiguration

		public override IEngine<M> GetClient<M>()
		{
			var requestContextFactory = RequestContextFactory(Host, Port, ConnectionPooling);
			return new RemoteEngineClient<M>(requestContextFactory);
		}

		internal static IRequestContextFactory RequestContextFactory(string host, int port, bool pooled, int maxPoolConnections = 10)
		{
			return pooled ? (IRequestContextFactory)new PooledConnectionRequestContextFactory(host, port,maxPoolConnections)
											: new DedicatedConnectionRequestContextFactory(host, port);
		}

	    #endregion
    }
}