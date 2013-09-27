using System;

namespace OrigoDB.Core
{

    public class Engine<TModel> : Engine, IEngine<TModel> where TModel : Model
    {

        public Engine(EngineConfiguration config) : base(Activator.CreateInstance<TModel>, config) { }

        public Engine(TModel model, EngineConfiguration config) : base(() => model, config) { }


        public new TResult Execute<TTargetModel,TResult>(Func<TTargetModel, TResult> query) where TTargetModel : Model
        {
            if (typeof(TTargetModel) == typeof(TModel)) return base.Execute(query);
            else return base.Execute<TModel,TResult>(m => query.Invoke(m.ChildFor<TTargetModel>()));
        }

        public void Execute<TTargetModel>(Command<TTargetModel> command) where TTargetModel : Model
        {
            if (typeof(TTargetModel) == typeof(TModel)) base.Execute(command);
            else
            {
                var wrapperCommand = new ChildModelCommand<TModel, TTargetModel>(command);
                base.Execute(wrapperCommand);
            }
        }

        public new TResult Execute<TTargetModel,TResult>(Query<TTargetModel, TResult> query) where TTargetModel:Model
        { 
            if (typeof(TTargetModel) == typeof(TModel)) return base.Execute(query);
            else return base.Execute(new ChildModelQuery<TModel,TTargetModel,TResult>(query));
        }


        public R Execute<M, R>(CommandWithResult<M, R> command) where M : Model
        {
            if (typeof(M) == typeof(TModel)) return (R) base.Execute(command);
            else
            {
                var wrapperCommand = new ChildModelCommandWithResult<TModel, M, R>(command);
                return (R) base.Execute(wrapperCommand);
            }
        }
    }
}
