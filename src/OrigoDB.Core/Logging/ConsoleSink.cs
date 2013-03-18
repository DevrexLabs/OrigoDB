using System;

namespace OrigoDB.Core.Logging
{
    public class ConsoleSink : LogSink
    {
        public override void WriteMessage(string message)
        {
            Console.Write(message);
        }
    }
}