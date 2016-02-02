using System;
using OrigoDB.Core.Journaling;

namespace OrigoDB.Core.Storage
{

    /// <summary>
    /// Wraps objects in JournalEntry and sends to an underlying writer.
    /// Responsible for maintaining the sequence number.
    /// </summary>
    public class JournalAppender : IDisposable
    {
        readonly IJournalWriter _writer;
        private ulong _nextEntryId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nextEntryId"></param>
        /// <param name="writer">The underlying writer</param>
        public JournalAppender(ulong nextEntryId, IJournalWriter writer)
        {
            _nextEntryId = nextEntryId;
            _writer = writer;
        }

        /// <summary>
        /// Append an object to the journal wrapped with a JournalEntry
        /// </summary>
        /// <returns>The id of the entry appended</returns>
        public ulong Append(Command item)
        {
            var ts = Execution.Current.Now;
            var entry = new JournalEntry<Command>(_nextEntryId, item, ts);
            _writer.Write(entry);
            return _nextEntryId++;
        }

        /// <summary>
        /// Append an entry of type ModelCreated to the journal
        /// </summary>
        /// <param name="modelType"></param>
        public void AppendModelCreated(Type modelType)
        {
            if (_nextEntryId != 0) throw new InvalidOperationException("ModelCreated entry must have Id = 0");
            _writer.Write(CreateEntry(new ModelCreated(modelType)));
        }

        /// <summary>
        /// Append an entry of type ModelCreated to the journal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AppendModelCreated<T>()
        {
            AppendModelCreated(typeof(T));
        }

        private JournalEntry<T> CreateEntry<T>(T item)
        {
            return new JournalEntry<T>(_nextEntryId++, item);
        }

        /// <summary>
        /// Rollback the previous command by writing a rollback marker.
        /// <remarks>Uses the same id as the rollbacked command to not break the sequence</remarks>
        /// </summary>
        public void AppendRollbackMarker()
        {
            if (_nextEntryId == 1) throw new InvalidOperationException("Can't rollback when entryId is 0");
            var entry = new JournalEntry<RollbackMarker>(_nextEntryId - 1, new RollbackMarker());
            _writer.Write(entry);
        }

        public ulong LastEntryId
        {
            get
            {
                return _nextEntryId - 1;
            }
        }

        public static JournalAppender Create(ulong nextEntryId, ICommandStore store)
        {
            return Create(nextEntryId, store.CreateJournalWriter(nextEntryId));
        }

        public static JournalAppender Create(ulong nextEntryId, IJournalWriter writer)
        {
            return new JournalAppender(nextEntryId, writer);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        internal void Handle(SnapshotCreated snapshotCreated)
        {
            _writer.Handle(snapshotCreated);
        }

    }
}