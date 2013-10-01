using System;
using System.Runtime.Serialization;

namespace OrigoDB.Core
{
    /// <summary>
    /// The command threw an unexpected exception
    /// </summary>
    [Serializable]
    public class CommandFailedException : Exception
    {
        /// <summary>
        /// Needed to deserialize because Exception implements ISerializable
        /// </summary>
        protected CommandFailedException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx){}

        public CommandFailedException()
            :this("Command threw an unexpected exception, see inner exception for details")
        {}
        public CommandFailedException(string message):base(message){}
        public CommandFailedException(string message, Exception innerException) 
            : base(message, innerException){}
    }
}
