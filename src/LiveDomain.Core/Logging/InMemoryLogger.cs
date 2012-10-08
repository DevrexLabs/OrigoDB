using System.Collections.Generic;

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

        internal void Clear()
        {
            _messages.Clear();
        }
    }
}
