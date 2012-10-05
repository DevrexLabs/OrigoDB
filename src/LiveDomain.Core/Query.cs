using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    [Serializable]
    public abstract class Query : ITransactionWithResult
    {
        internal abstract object ExecuteStub(Model m);


        protected Query(bool ensuresResultIsDisconnected = false)
        {
            EnsuresResultIsDisconnected = ensuresResultIsDisconnected;
        }

        /// <summary>
        /// True if results are safe to return to client, default is false. Set to true if your command implementation 
        /// gaurantees no references to mutable objects within the model are returned.
        /// </summary>
        public bool EnsuresResultIsDisconnected { get; internal protected set; }
    }

    [Serializable]
	public abstract class Query<M,R>:Query where M : Model
	{
        internal override object ExecuteStub(Model model)
        {
            return Execute(model as M);
        }
        protected abstract R Execute(M m);
	}
}
