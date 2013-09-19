using System;

namespace OrigoDB.Core.Logging
{
    public interface ILogger
    {
        void Trace(string messageTemplate, params object[] args);
        void Trace(Exception exception);
        void Trace(Func<string> messageGenerator);
        
        void Debug(string messageTemplate, params object[] args);
        void Debug(Exception exception);
        void Debug(Func<string> messageGenerator);
        
        void Info(string messageTemplate, params object[] args);
        void Info(Exception exception);
        void Info(Func<string> messageGenerator);

        void Warn(string message, params object[] args);
        void Warn(Exception exception);
        void Warn(Func<string> messageGenerator);
        
        void Error(string message, params object[] args);
        void Error(Exception exception);
        void Error(Func<string> messageGenerator);

        void Fatal(string message, params object[] args);
        void Fatal(Exception exception);
        void Fatal(Func<string> messageGenerator);
    }
}
