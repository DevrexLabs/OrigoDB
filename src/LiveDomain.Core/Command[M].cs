using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiveDomain.Core
{


    /// <summary>
    /// A command modifies the state of the model.
    /// </summary>
    /// <typeparam name="M">The specific type of the model, derived from Model</typeparam>
    [Serializable]
    public abstract class Command<M> : Command
        where M : Model
    {

        protected internal virtual void Prepare(M model) { }

        internal override object ExecuteStub(Model model)
        {
            Execute(model as M);
            return null;
        }

        internal override void PrepareStub(Model model)
        {
            try
            {
                Prepare(model as M);
            }
            catch (Exception ex)
            {
                
               throw new CommandFailedException("Exception thrown during Prepare(), see inner exception for details", ex);
            }
        }

        internal protected abstract void Execute(M model);
    }

}
