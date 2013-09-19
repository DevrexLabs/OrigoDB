using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Logging
{
    public class NullLogger : ILogger
    {

        public void Log(LogLevel level, string messageTemplate, params object[] args)
        {
        }

        public void Log(LogLevel level, Exception exception)
        {
        }

        public void Log(LogLevel level, Func<string> messageGenerator)
        {
        }

        public void Trace(string messageTemplate, params object[] args)
        {
        }

        public void Trace(Exception exception)
        {
        }

        public void Trace(Func<string> messageGenerator)
        {
        }

        public void Debug(string messageTemplate, params object[] args)
        {
        }

        public void Debug(Exception exception)
        {
        }

        public void Debug(Func<string> messageGenerator)
        {
        }

        public void Info(string messageTemplate, params object[] args)
        {
        }

        public void Info(Exception exception)
        {
        }

        public void Info(Func<string> messageGenerator)
        {
        }

        public void Warn(string message, params object[] args)
        {
        }

        public void Warn(Exception exception)
        {
        }

        public void Warn(Func<string> messageGenerator)
        {
        }

        public void Error(string message, params object[] args)
        {
        }

        public void Error(Exception exception)
        {
        }

        public void Error(Func<string> messageGenerator)
        {
        }

        public void Fatal(string message, params object[] args)
        {
        }

        public void Fatal(Exception exception)
        {
        }

        public void Fatal(Func<string> messageGenerator)
        {
        }
    }
}
