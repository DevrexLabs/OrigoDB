using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core.Storage
{

    /// <summary>
    /// Base class with common behavior for CommandStore implementations
    /// </summary>
    public abstract class CommandStore : Initializable, ICommandStore
    {
        protected static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();
        
        protected EngineConfiguration _config;
        protected IFormatter _formatter;

        /// <summary>
        /// Read the sequence of entries with id greater than or equal to a given entryId
        /// </summary>
        protected abstract IEnumerable<JournalEntry> GetJournalEntriesFromImpl(ulong entryId);

        /// <summary>
        /// Delete entries up to and including a specific revision
        /// </summary>
        /// <param name="revision"></param>
        public abstract void Truncate(ulong revision);
        
        /// <summary>
        /// Get an append-only stream for writing journal entries
        /// </summary>
        public abstract Stream CreateJournalWriterStream(ulong firstEntryId = 1);
        
        /// <summary>
        /// Override if necessary
        /// </summary>
        /// <param name="pointInTime"></param>
        /// <returns></returns>
        public virtual IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            return GetJournalEntriesFrom(0).TakeWhile(e => e.Created <= pointInTime);
        }


        protected virtual IJournalWriter CreateStoreSpecificJournalWriter()
        {
            return new StreamJournalWriter(_config, CreateJournalWriterStream);
        }


        protected CommandStore(EngineConfiguration config)
        {
            _config = config;
        }


        public IEnumerable<JournalEntry> GetJournalEntriesFrom(ulong entryId)
        {
            bool firstEntry = true;
            foreach (var entry in GetJournalEntriesFromImpl(entryId))
            {
                if (firstEntry && entry.Id > entryId && entryId > 0)
                {
                    string msg = String.Format("requested journal entry missing [{0}]", entryId);
                    throw new InvalidOperationException(msg);
                }
                firstEntry = false;
                yield return entry;
            }
        }

        /// <summary>
        /// Iterate all the entries in the journal
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<JournalEntry> GetJournalEntries()
        {
            return GetJournalEntriesFrom(0);
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

        /// <summary>
        /// True if the journal is empty
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                return !GetJournalEntries().Any();
            }
        }

        /// <summary>
        /// Throw an exception unless the journal is empty
        /// </summary>
        protected void AssertEmpty()
        {
            if (!IsEmpty) throw new InvalidOperationException("Journal must be empty");
        }

        /// <summary>
        /// Get commited commands beginning from a specific entry id (inclusive)
        /// </summary>
        public IEnumerable<JournalEntry<Command>> CommandEntriesFrom(ulong entryId)
        {
            return CommittedCommandEntries(() => GetJournalEntriesFromImpl(entryId));
        }

        /// <summary>
        /// Get committed commands from a point in time
        /// </summary>
        public IEnumerable<JournalEntry<Command>> CommandEntriesBeforeOrAt(DateTime pointInTime)
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
    }
}
