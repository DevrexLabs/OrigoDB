using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
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
