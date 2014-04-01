using System;

namespace OrigoDB.Core
{

    /// <summary>
    /// A command modifies the state of the model. 
    /// </summary>
    [Serializable]
    public abstract class Command
    {

        [NonSerialized] private DateTime? _timestamp;

        /// <summary>
        /// Point in time of command execution. A safe alternative to DateTime.Now
        /// <remarks>Will be assigned when executed by engine</remarks>
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                if(!_timestamp.HasValue) throw new InvalidOperationException("Timestamp must be set");
                return _timestamp.Value;
            }
            set
            {
                _timestamp = value;
            }
        }

        /// <summary>
        /// Gracefully cancels command execution by throwing a CommandAbortedException.
        /// The engine will rethrow the exception and proceed normally.
        /// </summary>
        
        protected void Abort(string message = "", Exception innerException = null)
        {
            throw new CommandAbortedException(message, innerException);
        }

        /// <summary>
        /// Perform read operations before the write lock is obtained.
        /// </summary>
        /// <param name="model"></param>
        internal abstract void PrepareStub(Model model);


        internal virtual void Redo(ref Model model)
        {
            PrepareStub(model);
            ExecuteStub(model);
        }

        internal abstract object ExecuteStub(Model model);
    }
}
