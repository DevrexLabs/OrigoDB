using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    [Serializable]
	public abstract class Query<M,R> where M : Model
	{
        public abstract R Execute(Model m);
	}
}
