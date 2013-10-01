using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    public class LocalEngineClient<M> : IEngine<M> where M : Model
    {

        public readonly Engine<M> Engine;

        public LocalEngineClient(Engine<M> engine)
        {
            Engine = engine;
        }

        //public T Execute<T>(Query<M, T> query)
        //{
        //    return Engine.Execute(query);
        //}

        //public void Execute(Command<M> command)
        //{
        //    Engine.Execute(command);
        //}

        //public T Execute<T>(CommandWithResult<M, T> command)
        //{
        //    return Engine.Execute<T>(command);
        //}

        //public T Execute<T>(Func<M, T> query)
        //{
        //    return Engine.Execute(query);
        //}


        public T Execute<S, T>(Query<S, T> query) where S : Model
        {
            return Engine.Execute<S,T>(query);
        }

        public void Execute<S>(Command<S> command) where S : Model
        {
            Engine.Execute(command);
        }

        public T Execute<S, T>(Command<S, T> command) where S : Model
        {
            return Engine.Execute(command);
        }

        public T Execute<S, T>(Func<S, T> command) where S : Model
        {
            return Engine.Execute(command);
        }
    }
}
