using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    [Serializable]
    public abstract class JournalEntry
    {
        public readonly long Id;

        public readonly DateTime Created;

        public JournalEntry(long id, DateTime? created  = null)
        {
            Created = created ?? DateTime.Now;
            Id = id;
        }

    }

	[Serializable]
	public class JournalEntry<T> : JournalEntry
	{
        public T Item { get; protected internal set; }
		
		public JournalEntry(long id, T item, DateTime? created = null) : base(id, created)
		{
            Item = item;
		}
	}


}
