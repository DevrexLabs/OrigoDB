using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    [Serializable]
    public abstract class Query
    {
        internal abstract object ExecuteStub(Model m);
    }

    [Serializable]
	public abstract class Query<M,R>:Query where M : Model
	{
        internal override object ExecuteStub(Model m)
        {
            return Execute(m as M);
        }
        protected abstract R Execute(M m);
	}
}
