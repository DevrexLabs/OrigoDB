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

        public void Execute<S>(Command<S> command) where S : Model
        {
            if (typeof(S) == typeof(M)) base.Execute(command);
            else
            {
                var wrapperCommand = new ChildModelCommand<M, S>(command);
                base.Execute(wrapperCommand);
            }
        }


        public T Execute<T>(Query<M, T> query)
        {
            return (T)base.Execute(query);
        }


        public R Execute<S, R>(CommandWithResult<S, R> command) where S : Model
        {
            if (typeof(S) == typeof(M))
            {
                return (R) base.Execute(command);
            }
            else
            {
                var wrapperCommand = new ChildModelCommandWithResult<M, S, R>(command);
                return this.Execute<R>(wrapperCommand);
            }
        }
    }
}
