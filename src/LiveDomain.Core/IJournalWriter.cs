using System;
namespace LiveDomain.Core
{
	interface IJournalWriter : IDisposable
    {
        void Write(JournalEntry item);
    	void Close();
    }
}
