using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrigoDB.Core.Journaling;

namespace OrigoDB.Core
{

    [Serializable]
    public abstract class JournalEntry
    {
        public readonly ulong Id;

        public readonly DateTime Created;

        protected JournalEntry(ulong id, DateTime? created  = null)
        {
            Created = created ?? DateTime.Now;
            Id = id;
        }

    }


    /// <summary>
    /// 
    /// </summary>
	[Serializable]
	public class JournalEntry<T> : JournalEntry
	{
        public T Item { get; protected internal set; }
		
		public JournalEntry(ulong id, T item, DateTime? created = null) : base(id, created)
		{
		    if (typeof (T) != typeof (Command) && typeof (T) != typeof (RollbackMarker))
		    {
		        throw new InvalidOperationException("Invalid journal entry item type: " + typeof(T));
		    }
            Item = item;
		}
	}


}
