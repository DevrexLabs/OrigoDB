using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    /// <summary>
    /// A command failed but the model was not modified. Throw this exception from your commands execute method to cancel.
    /// </summary>
	[Serializable]
    public class CommandFailedException : Exception
    {
        public CommandFailedException()
        {

        }
        public CommandFailedException(string message):base(message)
        {

        }
        public CommandFailedException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
