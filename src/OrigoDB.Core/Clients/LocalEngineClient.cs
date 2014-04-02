using System;

namespace OrigoDB.Core
{
    public class LocalEngineClient<TModel> : IEngine<TModel> where TModel : Model
    {

        public readonly Engine<TModel> Engine;

        public LocalEngineClient(Engine<TModel> engine)
        {
            Engine = engine;
        }

        public TResult Execute<TResult>(Query<TModel, TResult> query)
        {
            return Engine.Execute(query);
        }

        public void Execute(Command<TModel> command)
        {
            Engine.Execute(command);
        }

        public TResult Execute<TResult>(Command<TModel, TResult> command)
        {
            return Engine.Execute(command);
        }

        public TResult Execute<TResult>(Func<TModel, TResult> query)
        {
            return Engine.Execute(query);
        }
    }
}
