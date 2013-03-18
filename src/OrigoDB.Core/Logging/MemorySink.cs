using System.Collections.Generic;

namespace OrigoDB.Core.Logging
{
    public class MemorySink : LogSink
    {

        public const int DefaultMaxMessages = 1000;
        public readonly int MaxMessages;

        private readonly LinkedList<string> _messages;

        public MemorySink(int maxMessages = DefaultMaxMessages)
        {
            MaxMessages = maxMessages;
            _messages = new LinkedList<string>();
        }


        public IEnumerable<string> Messages
        {
            get
            {
                foreach (string message in _messages)
                {
                    yield return message;
                }
            }
        }

        internal void Clear()
        {
            _messages.Clear();
        }

        public override void WriteMessage(string message)
        {
            if(_messages.Count == MaxMessages) _messages.RemoveFirst();
            _messages.AddLast(message);
        }
    }
}
