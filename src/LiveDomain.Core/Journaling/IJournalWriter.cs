using System;
namespace LiveDomain.Core
{
	public interface IJournalWriter : IDisposable
    {
        long Length { get; }
        void Write(JournalEntry item);
    	void Close();
    }
}
