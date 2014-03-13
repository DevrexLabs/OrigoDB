using System;

namespace OrigoDB.Core
{
    [Serializable]
	public abstract class Query<M,R>:Query where M : Model
	{
        internal override object ExecuteStub(Model model)
        {
            return Execute(model as M);
        }

        public abstract R Execute(M model);
	}
}
