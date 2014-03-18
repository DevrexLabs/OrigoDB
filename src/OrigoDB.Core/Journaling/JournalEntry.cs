using System;

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


	[Serializable]
	public class JournalEntry<T> : JournalEntry
	{
        public T Item { get; protected internal set; }
		
		public JournalEntry(ulong id, T item, DateTime? created = null) : base(id, created)
		{
            if (item is Command && typeof(T) != typeof(Command)) throw new InvalidOperationException();
            Item = item;
		}
	}


}
