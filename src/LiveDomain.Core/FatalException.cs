using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	[Serializable]
    public class FatalException : Exception
    {
        public FatalException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
