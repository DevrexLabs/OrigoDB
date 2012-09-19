using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Woocode.Utils;

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

	    private class ModeSetting
	    {
			public Mode Mode { get; set; } 
	    }

		public static ClientConfiguration Create(EngineConfiguration config)
		{
			return new LocalClientConfiguration(config);
		}

	    public static ClientConfiguration Create(string clientIdentifier = null)
		{
			if(clientIdentifier == null)
				return new LocalClientConfiguration(EngineConfiguration.Create());
		
			var isConnectionString = clientIdentifier.Contains("=");
			if(isConnectionString)
				 return CreateConfigFromConnectionString(clientIdentifier);
			else
			{
				var config = EngineConfiguration.Create();
				config.Location = clientIdentifier;
				return new LocalClientConfiguration(config);
			}
		}

		static ClientConfiguration CreateConfigFromConnectionString(string connectionstring)
	    {
			var modeSetting = new ModeSetting();
			var mapper = new StringToPropertiesMapper();
			
			mapper.MapProperties(connectionstring,modeSetting,false);

			if(modeSetting.Mode == Mode.Embedded)
			{
				var config = EngineConfiguration.Create();
				mapper.MapProperties(connectionstring,config,false);
				return new LocalClientConfiguration(config);
			}
			else if(modeSetting.Mode == Mode.Remote)
			{
				var config = new RemoteClientConfiguration();
				mapper.MapProperties(connectionstring,config,false);
				return config;
			}

		    throw new InvalidOperationException();
	    }

	    public abstract IEngine<M> GetClient<M>() where M : Model, new();
    }
}
