using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Woocode.Utils;
using System.Collections.Specialized;

namespace LiveDomain.Core
{
    public abstract class ClientConfiguration : ConfigurationBase
    {
	    private enum Mode
	    {
			Missing,
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
			// Todo: Check config for client identifier.

			var isConnectionString = clientIdentifier.Contains("=");
			if(isConnectionString)
				 return CreateConfigFromConnectionString(clientIdentifier);
			else
			{
				var config = EngineConfiguration.Create();
				config.Location.OfJournal = clientIdentifier;
				return new LocalClientConfiguration(config);
			}
		}

        /// <summary>
        /// Used as a container for the enum so we can use the DictionaryMapper
        /// </summary>
        private class ModeSetting
        {
            public Mode Mode { get; set; }
        }

		static ClientConfiguration CreateConfigFromConnectionString(string connectionstring)
		{
		    var dictionary = connectionstring.ParseProperties();
            var mapper = new DictionaryMapper(dictionary);
            
            var modeSetting = new ModeSetting();
            mapper.Map(modeSetting);

            if (modeSetting.Mode == Mode.Embedded)
            {
                mapper.Converters[typeof(StorageLocation)] = s => new FileStorageLocation(s);
                var config = EngineConfiguration.Create();
                mapper.Map(config);
                return new LocalClientConfiguration(config);
            }
            else if (modeSetting.Mode == Mode.Remote)
            {
                var config = new RemoteClientConfiguration();
                mapper.Map(config);
                return config;
            }

            throw new InvalidOperationException();
		}

	    public abstract IEngine<M> GetClient<M>() where M : Model, new();
    }
}
