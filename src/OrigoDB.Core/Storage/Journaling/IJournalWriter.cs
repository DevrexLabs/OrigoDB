using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// Journal writers write JournalEntry objects to the journal
    /// </summary>
	public interface IJournalWriter : IDisposable
    {
        void Write(JournalEntry item);
    	
        void Close();
    }
}
