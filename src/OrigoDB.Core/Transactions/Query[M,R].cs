using System;

namespace OrigoDB.Core
{
    [Serializable]
	public abstract class Query<TModel,TResult>:Query where TModel : Model
	{
        internal override object ExecuteStub(Model model)
        {
            return Execute(model as TModel);
        }

        public abstract TResult Execute(TModel model);
	}
}
