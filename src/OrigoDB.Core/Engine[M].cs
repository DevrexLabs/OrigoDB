using System;
using System.Runtime.InteropServices;

namespace OrigoDB.Core
{

    public class Engine<TModel> : Engine, IEngine<TModel> where TModel : Model
    {


        public Engine(TModel model, IStore store, EngineConfiguration config) : base(model, store, config) { }


        public TResult Execute<TResult>(Func<TModel, TResult> query)
        {
            return Execute(new DelegateQuery<TModel, TResult>(query));
        }

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
