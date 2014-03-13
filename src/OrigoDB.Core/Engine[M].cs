using System;

namespace OrigoDB.Core
{

    public class Engine<TModel> : Engine, IEngine<TModel> where TModel : Model
    {


        public Engine(IStore store, EngineConfiguration config) : base(store, config) { }


        public TResult Execute<TResult>(Query<TModel, TResult> query)
        {
            return base.Execute(query);
        }

        public void Execute(Command<TModel> command)
        {
            base.Execute(command);
        }

        public TResult Execute<TResult>(Command<TModel, TResult> command)
        {
            return (TResult) base.Execute(command);
        }
    }
}
