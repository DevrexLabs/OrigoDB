using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public class LocalEngineClient<M> : ILocalEngine<M> where M : Model
    {

        public readonly Engine<M> Engine;

        public LocalEngineClient(Engine<M> engine)
        {
            Engine = engine;
        }

        public T Execute<T>(Query<M, T> query)
        {
            return Engine.Execute(query);
        }

        public void Execute(Command<M> command)
        {
            Engine.Execute(command);
        }

        public T Execute<T>(CommandWithResult<M, T> command)
        {
            return Engine.Execute<T>(command);
        }

	    public T Execute<T>(Func<M, T> query)
	    {
		    return Engine.Execute(query);
	    }


        public T Execute<S, T>(Query<S, T> query) where S : Model
        {
            return Engine.Execute(query);
        }

        public void Execute<S>(Command<S> command) where S : Model
        {
            Engine.Execute(command);
        }

        public T Execute<S, T>(CommandWithResult<S, T> command) where S : Model
        {
            return Engine.Execute(command);
        }
    }
}
