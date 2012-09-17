using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Woocode.Utils;

namespace LiveDomain.Core
{
    public abstract class ClientConfiguration : ConfigurationBase
    {
	    enum Mode
	    {
			Missing,
		    Embedded,
			Remote
	    }

	    class Settings
	    {
			public Mode Mode { get; set; } 
	    }

		public static ClientConfiguration Create(string clientIdentifier = null)
		{
			if(clientIdentifier == null)
				return new LocalClientConfiguration(EngineConfiguration.Create());
		
			var isConnectionString = clientIdentifier.Contains("=");
			if(isConnectionString)
				 return GetConfigFromConnectionString(clientIdentifier);

			return null;
		}

	    static ClientConfiguration GetConfigFromConnectionString(string connectionstring)
	    {
		    var baseSettings = new Settings();
			var mapper = new StringToPropertiesMapper();
			
			mapper.MapProperties(connectionstring,baseSettings,false);

			if(baseSettings.Mode == Mode.Embedded)
			{
				var config = EngineConfiguration.Create();
				mapper.MapProperties(connectionstring,config);
				return new LocalClientConfiguration(config);
			}
			else if(baseSettings.Mode == Mode.Remote)
			{
				var config = new RemoteClientConfiguration();
				mapper.MapProperties(connectionstring,config);
				return config;
			}

		    throw new InvalidOperationException();
	    }

	    public abstract IEngine<M> GetClient<M>() where M : Model, new();
    }
}
