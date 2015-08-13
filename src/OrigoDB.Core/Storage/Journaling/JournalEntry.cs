using System;
using System.Runtime.Serialization;
using OrigoDB.Core.Journaling;

namespace OrigoDB.Core
{

    [Serializable]
    public abstract class JournalEntry : ISerializable
    {
        /// <summary>
        /// Corresponds to the <see cref="Model.Revision"/> resulting after this entry is applied
        /// </summary>
        public readonly ulong Id;

        public readonly DateTime Created;

        protected JournalEntry(ulong id, DateTime? created = null)
        {
            Created = created ?? DateTime.Now;
            Id = id;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id);
            info.AddValue("Created", Created);
        }

        private static bool? _unsignedIdsInJournal = false;

        protected JournalEntry(SerializationInfo info, StreamingContext context)
        {
            Created = info.GetDateTime("Created");
            if (!_unsignedIdsInJournal.HasValue)
            {
                try
                {
                    Id = info.GetUInt64("Id");
                    _unsignedIdsInJournal = true;
                }
                catch (Exception)
                {
                    Id = (ulong) info.GetInt64("Id");
                    _unsignedIdsInJournal = false;
                }
            }
            else if (_unsignedIdsInJournal.Value) Id = info.GetUInt64("Id");
            else Id = (ulong) info.GetInt64("Id");
        }


        internal abstract object GetItem();

        internal static JournalEntry Create(ulong id, DateTime created, object item)
        {
            if (item is ModelCreated) return new JournalEntry<ModelCreated>(id, (ModelCreated) item, created);
            if (item is RollbackMarker) return new JournalEntry<RollbackMarker>(id, (RollbackMarker) item, created);
            if (item is Command) return new JournalEntry<Command>(id, (Command) item, created);
            throw new ArgumentOutOfRangeException("unrecognized journal entry item :" + item);
        }
    }


    [Serializable]
    public class JournalEntry<T> : JournalEntry
    {
        internal override object GetItem()
        {
            return Item;
        }

        public T Item { get; protected internal set; }

        public JournalEntry(ulong id, T item, DateTime? created = null)
            : base(id, created)
        {
            if (item is Command && typeof(T) != typeof(Command)) throw new InvalidOperationException();
            Item = item;
        }

        protected JournalEntry(SerializationInfo info, StreamingContext context)
            :base(info,context)
        {
            Item = (T)info.GetValue("<Item>k__BackingField", typeof(T));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("<Item>k__BackingField", Item, typeof(T));
        }
    }


}
