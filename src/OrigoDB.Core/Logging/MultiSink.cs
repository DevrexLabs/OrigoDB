using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Logging
{
    public class MultiSink : LogSink
    {
        public readonly List<LogSink> Sinks;

        public MultiSink(params LogSink[] sinks)
        {
            Sinks = new List<LogSink>(sinks);
        }


        public override void Write(string logger, LogLevel level, string message)
        {
            foreach (LogSink logSink in Sinks)
            {
                logSink.Write(logger,level,message);
            }
        }

        public override void WriteMessage(string formattedMessage)
        {
            foreach(LogSink sink in Sinks) sink.WriteMessage(formattedMessage);
        }
    }
}
