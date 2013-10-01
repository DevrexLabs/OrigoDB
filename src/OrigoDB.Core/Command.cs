using System;

namespace OrigoDB.Core
{

    /// <summary>
    /// A command modifies the state of the model.
    /// </summary>
    [Serializable]
    public abstract class Command
    {

        /// <summary>
        /// Cancels the command by throwing a CommandAbortedException.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        protected void Abort(string message = "", Exception innerException = null)
        {
            throw new CommandAbortedException(message, innerException);
        }

        /// <summary>
        /// Perform read operations before the write lock is obtained.
        /// </summary>
        /// <param name="model"></param>
        internal abstract void PrepareStub(Model model);


        internal void Redo(Model model)
        {
            PrepareStub(model);
            ExecuteStub(model);
        }

        internal abstract object ExecuteStub(Model model);
    }
}
