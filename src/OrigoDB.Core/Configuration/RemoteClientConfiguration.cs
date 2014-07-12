
namespace OrigoDB.Core
{
	public class RemoteClientConfiguration : ClientConfiguration
    {
		public static readonly RemoteClientConfiguration Default = new RemoteClientConfiguration(); 
	    public const int DefaultMaxConnections = 10;
	    public const string DefaultHost = "localhost";
	    public const int DefaultPort = 3001;
		
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
		    return string.Format("Host={0};Port={1};MaxConnections={2};DedicatedPool={3}", Host, Port, MaxConnections,
		        DedicatedPool);
		}

		public override IEngine<TModel> GetClient<TModel>()
		{
			var pool = ConnectionPools.PoolFor(this);
			var requestContextFactory = new PooledConnectionRequestContextFactory(pool);
			return new RemoteEngineClient<TModel>(requestContextFactory,Host,Port);
		} 
    }
}