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
        public int MaxPoolConnections { get; set; }

        public RemoteClientConfiguration()
        {
            Host = "localhost";
            Port = 9292;
            ConnectionPooling = true;
            MaxPoolConnections = 10;
        }

	    #region Overrides of ClientConfiguration

		public override IEngine<M> GetClient<M>()
		{
			var requestContextFactory = ConnectionPooling
									? (IRequestContextFactory)
									  new PooledConnectionRequestContextFactory(ConnectionPools.GetPoolFor(Host, Port, MaxPoolConnections))
									: new DedicatedConnectionRequestContextFactory(new TcpClient(Host, Port));

			return new RemoteEngineClient<M>(requestContextFactory);
		}

	    #endregion
    }
}