using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public abstract class ClientConfiguration : ConfigurationBase
    {
        public static ClientConfiguration Create()
        {
            throw new NotImplementedException();
        }

        public abstract IEngine<M> GetClient<M>(string engineIdentifier) where M : Model, new();
    }

    public class LocalClientConfiguration : ClientConfiguration
    {
        private EngineConfiguration _engineConfiguration;

        public LocalClientConfiguration(EngineConfiguration engineConfiguration)
        {
            _engineConfiguration = engineConfiguration;
        }

        public bool CreateWhenNotExists { get; set; }


        public override IEngine<M> GetClient<M>(string engineIdentifier)
        {
            //TODO: do smart stuff with engineIdentifier. name from config, connectionstring or location.
            if (CreateWhenNotExists) return Engine.LoadOrCreate<M>(engineIdentifier);
            else return Engine.Load<M>(engineIdentifier);
        }
    }
}
