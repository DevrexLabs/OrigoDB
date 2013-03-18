using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Journaling
{
    public class NullJournalWriter : IJournalWriter
    {
        public void Write(JournalEntry item)
        {
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}