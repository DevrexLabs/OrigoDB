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

        internal override void PrepareStub(Model model)
        {
            Prepare(model as M);
        }

        internal override object ExecuteStub(Model model)
        {
            return Execute(model as M);
        }

        internal protected abstract R Execute(M model);
    }
}
