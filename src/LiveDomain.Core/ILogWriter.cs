using System;
namespace LiveDomain.Core
{
	public interface ILogWriter : IDisposable
    {
        void Write(LogItem logItem);
    	void Close();
    }
}
