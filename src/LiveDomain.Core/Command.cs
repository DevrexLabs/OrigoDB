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
    [Serializable]
    public abstract class Command : ILogCommand
    {

        /// <summary>
        /// Perform read operations before the write lock is obtained.
        /// </summary>
        /// <param name="model"></param>
        internal abstract void PrepareStub(Model model);

        void ILogCommand.Redo(Model model)
        {
            PrepareStub(model);
            ExecuteStub(model);
        }

        internal abstract void ExecuteStub(Model model);

    }

    /// <summary>
    /// A command modifies the state of the model.
    /// </summary>
    /// <typeparam name="M">The specific type of the model, derived from Model</typeparam>
    [Serializable]
    public abstract class Command<M> : Command 
        where M : Model
    {

        protected internal virtual void Prepare(M model) {}

        internal override void ExecuteStub(Model model)
        {
            Execute(model as M);
        }

        internal override void PrepareStub(Model model)
        {
            Prepare(model as M);
        }

        internal protected abstract void Execute(M model);

    }

    [Serializable]
    public abstract class CommandWithResult<M, R> : Command
        where M : Model
    {
        internal protected virtual void Prepare(M model) { }

        internal override void PrepareStub(Model model)
        {
            Prepare(model as M);
        }

        internal override void ExecuteStub(Model model)
        {
            Execute(model as M);
        }

        internal protected abstract R Execute(M model);
    }
}
