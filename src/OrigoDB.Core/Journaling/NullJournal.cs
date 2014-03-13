using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core
{
    public class NullJournal //: ICommandJournal
    {

        public IEnumerable<JournalEntry<Command>> GetAllEntries()
        {
            return Enumerable.Empty<JournalEntry<Command>>();
        }

        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(DateTime pointInTime)
        {
            return Enumerable.Empty<JournalEntry<Command>>();
        }

        public IEnumerable<JournalEntry<Command>> GetEntriesFrom(long sequenceNumber)
        {
            return Enumerable.Empty<JournalEntry<Command>>();
        }

        public void Append(Command command)
        {
            
        }

        public long LastEntryId
        {
            get { return -1; }
        }

        public void WriteRollbackMarker()
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}