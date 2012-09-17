using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public class Engine<M> : Engine, ILocalEngine<M> where M : Model
    {

        public Engine(EngineConfiguration config) : base(() => Activator.CreateInstance<M>(), config) { }


        public T Execute<T>(Func<M, T> query)
        {
            return base.Execute(query);
        }

        public T Execute<T>(CommandWithResult<M, T> command)
        {
            return (T)base.Execute(command);
        }

        public void Execute(Command<M> command)
        {
            base.Execute(command);
        }

        public T Execute<T>(Query<M, T> query)
        {
            return (T)base.Execute(query);
        }


        public IEngine<M> For<M>() where M:Model, new()
        {
            return For<M>(typeof (M).Name);
        }

        public IEngine<M> For<M>(string engineIdentifier) where M : Model, new()
        {
            //Could return an EnterpriseClientConfiguration
            var config = ClientConfiguration.Create();
            return config.GetClient<M>(engineIdentifier);
        }
    }

}
