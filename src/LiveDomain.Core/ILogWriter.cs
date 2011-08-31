using System;
namespace LiveDomain.Core
{
    interface ILogWriter : IDisposable
    {
        void Write(LogItem logItem);
    	void Close();
    }
}
