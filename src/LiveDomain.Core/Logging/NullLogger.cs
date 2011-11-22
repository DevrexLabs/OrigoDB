using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public class NullLogger : Logger
    {
        protected override void WriteToLog(string message)
        {
        }
    }
}
