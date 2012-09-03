using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public class Engine<M> : Engine, ILocalTransactionHandler<M> where M : Model
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
    }

}
