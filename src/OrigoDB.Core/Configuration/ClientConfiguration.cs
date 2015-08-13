using System;
using System.Configuration;
using System.Text;

namespace OrigoDB.Core
{
    public abstract class ClientConfiguration
    {
	    private enum Mode
	    {
		    Embedded,
			Remote
	    }

		public static ClientConfiguration Create(EngineConfiguration config)
		{
			return new LocalClientConfiguration(config);
		}

	    public static ClientConfiguration Create(string clientIdentifier = null)
		{
			if(string.IsNullOrEmpty(clientIdentifier))
				return new LocalClientConfiguration(EngineConfiguration.Create());

			var isConnectionString = clientIdentifier.Contains("=");
			if(isConnectionString)
				 return CreateConfigFromConnectionString(clientIdentifier);
            
            if (ConfigurationManager.AppSettings[clientIdentifier] != null)
                return Create(ConfigurationManager.AppSettings[clientIdentifier]);

            var config = EngineConfiguration.Create();
			config.Location.OfJournal = clientIdentifier;
			return new LocalClientConfiguration(config);
		}

		static ClientConfiguration CreateConfigFromConnectionString(string connectionstring)
		{
		    var configDictionary = ConfigDictionary.FromDelimitedString(connectionstring);
            

		    var mode = configDictionary.Get("mode", () => Mode.Embedded);

		    Func<string, bool> keyFilter = key => key.ToLowerInvariant() != "mode";

            if (mode == Mode.Embedded)
            {
                Utils.Converters[typeof(StorageLocation)] = s => new FileStorageLocation(s);
                var config = EngineConfiguration.Create();
                configDictionary.MapTo(config, keyFilter: keyFilter);
                return new LocalClientConfiguration(config);
            }
            else //Mode.Remote
            {
                var config = new RemoteClientConfiguration();
                configDictionary.MapTo(config, keyFilter: keyFilter);
				return new FailoverClusterClientConfiguration(config);
            }
		}

	    public abstract IEngine<TModel> GetClient<TModel>() where TModel : Model, new();
    }
}
