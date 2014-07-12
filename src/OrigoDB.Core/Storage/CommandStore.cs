using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core.Storage
{

    public abstract class CommandStore : Initialized, ICommandStore
    {
        protected static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();
        
        protected EngineConfiguration _config;
        protected IFormatter _formatter;


        protected abstract IJournalWriter CreateStoreSpecificJournalWriter();
        public abstract IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId);
        public abstract IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime);
        public abstract Stream CreateJournalWriterStream(ulong firstEntryId = 1);


        protected CommandStore(EngineConfiguration config)
        {
            _config = config;
        }



        public virtual IEnumerable<JournalEntry> GetJournalEntries()
        {
            return GetJournalEntriesFrom(0);
        }


        public JournalAppender CreateAppender(ulong nextRevision)
        {
            return new JournalAppender(nextRevision, CreateJournalWriter(nextRevision));
        }

        public virtual IJournalWriter CreateJournalWriter(ulong lastEntryId)
        {
            IJournalWriter writer = CreateStoreSpecificJournalWriter();
            return _config.AsynchronousJournaling
                       ? new AsynchronousJournalWriter(writer)
                       : writer;
        }

        public override void Initialize()
        {
            _formatter = _config.CreateFormatter(FormatterUsage.Journal);
            base.Initialize();
        }

        public bool IsEmpty
        {
            get
            {
                return !GetJournalEntries().Any();
            }
        }

        protected void AssertEmpty()
        {
            if (!IsEmpty) throw new InvalidOperationException("Store must be empty");
        }

        /// <summary>
        /// Get commands beginning from a specific entry id (inclusive)
        /// </summary>
        public IEnumerable<JournalEntry<Command>> CommandEntriesFrom(ulong entryId)
        {
            return CommittedCommandEntries(() => GetJournalEntriesFrom(entryId));
        }

        /// <summary>
        /// Get non rolled back commands from a point in time
        /// </summary>
        public IEnumerable<JournalEntry<Command>> CommandEntriesFrom(DateTime pointInTime)
        {
            return CommittedCommandEntries(() => GetJournalEntriesBeforeOrAt(pointInTime));
        }

        /// <summary>
        /// Select the items of type Command that are not followed by a rollback marker
        /// </summary>
        public static IEnumerable<JournalEntry<Command>> CommittedCommandEntries(Func<IEnumerable<JournalEntry>> enumerator)
        {

            //rollback markers repeat the id of the rolled back command
            JournalEntry<Command> previous = null;

            foreach (JournalEntry current in enumerator.Invoke())
            {
                if (current is JournalEntry<Command>)
                {
                    (current as JournalEntry<Command>).Item.Timestamp = current.Created;
                    if (previous != null) yield return previous;
                    previous = (JournalEntry<Command>)current;
                }
                else previous = null;
            }
            if (previous != null) yield return previous;
        }

        /// <summary>
        /// Get the complete sequence of commands skipping any that were rolled back
        /// </summary>
        public IEnumerable<JournalEntry<Command>> CommandEntries()
        {
            return CommandEntriesFrom(1);
        }
        //public virtual void Create<T>() where T : Model, new()
        //{
        //    AssertEmpty();
        //    Create(typeof(T));
        //}

        //public virtual void Create(Type modelType)
        //{
        //    AssertEmpty();
        //    var writer = CreateJournalWriter(0);
        //    var appender = new JournalAppender(1, writer);
        //    appender.AppendModelCreated(modelType);
        //    writer.Close();
        //}

        //public virtual void Create(Model model)
        //{
        //    AssertEmpty();
        //    WriteSnapshotImpl(model, 0);
        //}

    }
}
