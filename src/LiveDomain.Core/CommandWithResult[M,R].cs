using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiveDomain.Core
{

    [Serializable]
    public abstract class CommandWithResult<M, R> : Command
        where M : Model
    {
        internal protected virtual void Prepare(M model) { }


        //TODO: Duplicate method! refactor
        internal override void PrepareStub(Model model)
        {
            try
            {
                Prepare(model as M);
            }
            catch (Exception ex)
            {

                throw new CommandAbortedException("Exception thrown during Prepare(), see inner exception for details", ex);
            }
        }

        internal override object ExecuteStub(Model model)
        {
            return Execute(model as M);
        }

        internal protected abstract R Execute(M model);
    }
}
