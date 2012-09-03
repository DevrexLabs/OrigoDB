using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LiveDomain.Core
{

    /// <summary>
    /// Throw this Exception from within Command.Execute(M model) to abort the command.
    /// An aborted command isn't written to the journal and must not have modified the model.
    /// <remarks>Throwing any other exception from within Command.Execute() will trigger a rollback, 
    /// restoring the model to the consistent state prior to the command</remarks>
    /// </summary>
	[Serializable]
    public class CommandAbortedException : Exception
    {
        /// <summary>
        /// Needed to deserialize because Exception implements ISerializable
        /// </summary>
        protected CommandAbortedException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx){}

        public CommandAbortedException(){}
        public CommandAbortedException(string message):base(message){}
        public CommandAbortedException(string message, Exception innerException) 
            : base(message, innerException){}
    }
}
