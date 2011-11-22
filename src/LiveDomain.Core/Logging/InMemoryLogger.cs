using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public class InMemoryLogger : Logger
    {
        List<string> _messages = new List<string>();
        public IEnumerable<string> Messages { get { return _messages; } }


        protected override void WriteToLog(string message)
        {
            _messages.Add(message);
        }
    }
}
