using System;

namespace OrigoDB.Core
{
	public interface IJournalWriter : IDisposable
    {
        void Write(JournalEntry item);
    	void Close();
    }
}
