using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public class Engine<T> : Engine, ILocalEngine<T> where T : Model
    {

        public Engine(EngineConfiguration config) : base(() => Activator.CreateInstance<T>(), config) { }


        public new R Execute<M,R>(Func<M, R> query) where M : Model
        {
            if (typeof(M) == typeof(T)) return base.Execute(query);
            else return base.Execute<T,R>(m => query.Invoke(m.ChildFor<M>()));
        }

        public void Execute<M>(Command<M> command) where M : Model
        {
            if (typeof(M) == typeof(T)) base.Execute(command);
            else
            {
                var wrapperCommand = new ChildModelCommand<T, M>(command);
                base.Execute(wrapperCommand);
            }
        }

        public new R Execute<M,R>(Query<M, R> query) where M:Model
        { 
            if (typeof(M) == typeof(T)) return base.Execute(query);
            else return base.Execute(new ChildModelQuery<T,M,R>(query));
        }


        public R Execute<M, R>(CommandWithResult<M, R> command) where M : Model
        {
            if (typeof(M) == typeof(T)) return (R) base.Execute(command);
            else
            {
                var wrapperCommand = new ChildModelCommandWithResult<T, M, R>(command);
                return (R) base.Execute(wrapperCommand);
            }
        }
    }
}
