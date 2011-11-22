using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public enum LogMessageType
    {
        Debug,
        Info,
        Warning,
        Exception
    }

    public interface ILog : IDisposable
    {
        void Debug(string message);
        void Write(string message);
        void Warn(string message);
        void Write(Exception exception);
    }





}
