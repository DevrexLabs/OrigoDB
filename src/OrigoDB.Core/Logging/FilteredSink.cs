using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Logging
{
    internal class FilteredSink : LogSink
    {
        readonly Func<string, LogLevel, string, bool> _predicate;

        public readonly LogSink _decoratedSink;

        public FilteredSink(LogSink sink, Func<string, LogLevel, string, bool> predicate = null)
        {
            _decoratedSink = sink;
            _predicate = predicate ?? ((logger, level, message) => true);
        }

        public override void WriteMessage(string formattedMessage)
        {
            throw new InvalidOperationException("Can't call WriteMessage on a FilteredSink, properties to filter are no longer available");
        }

        public override void Write(string logger, LogLevel level, string message)
        {
            if (_predicate.Invoke(logger, level, message))
            {
                _decoratedSink.Write(logger, level, message);
            }
        }
    }
}
