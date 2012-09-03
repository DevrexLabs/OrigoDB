using System;
namespace LiveDomain.Core
{
	public interface IJournalWriter : IDisposable
    {
        void Write(JournalEntry item);
    	void Close();
    }
}
