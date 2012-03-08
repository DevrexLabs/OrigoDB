using System;

namespace LiveDomain.Core.Logging
{

    public enum LogMessageType
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    
    public interface ILog : IDisposable
    {
        void Trace(string message, params object[] args);
        void Debug(string message, params object[] args);
        void Info(string message, params object[] args);
        void Warn(string message, params object[] args);
        void Error(string message, params object[] args);
        void Exception(Exception exception);
        void Fatal(string message, params object[] args);
    }





}
