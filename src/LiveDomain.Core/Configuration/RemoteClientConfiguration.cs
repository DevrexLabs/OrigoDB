using Woocode.Utils;

namespace LiveDomain.Core
{
	public class RemoteClientConfiguration : ClientConfiguration
    {
		public static readonly RemoteClientConfiguration Default = new RemoteClientConfiguration(); 
	    public const int DefaultMaxConnections = 10;
	    public const string DefaultHost = "localhost";
	    public const int DefaultPort = 9292;
		
		public string Host { get; internal set; }
		public int Port { get; internal set; }
		/// <summary>
		/// Maximum number of open Tcp connections to Host per pool.
		/// </summary>
        public int MaxConnections { get; internal set; }
		public bool DedicatedPool { get; internal set; }

        public RemoteClientConfiguration()
        {
	        Host = DefaultHost;
            Port = DefaultPort;
			MaxConnections = DefaultMaxConnections;
        }

		public override string ToString()
		{
			return new StringToPropertiesMapper().ToPropertiesString(this, Default);
		}

		public override IEngine<M> GetClient<M>()
		{
			var pool = ConnectionPools.PoolFor(this);
			var requestContextFactory = new PooledConnectionRequestContextFactory(pool);
			return new RemoteEngineClient<M>(requestContextFactory);
		} 
    }
}