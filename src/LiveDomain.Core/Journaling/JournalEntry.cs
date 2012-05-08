using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    [Serializable]
    public abstract class JournalEntry
    {
        protected internal readonly DateTime Created;
        public JournalEntry()
        {
            Created = DateTime.Now;
        }

    }

	[Serializable]
	public class JournalEntry<T> : JournalEntry
	{
        public T Item { get; protected internal set; }
		
		internal JournalEntry(T item)
		{
            Item = item;
		}
	}
}
