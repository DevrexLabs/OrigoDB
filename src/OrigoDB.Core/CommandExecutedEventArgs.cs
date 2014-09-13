using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core
{
    /// <summary>
    /// A command has just executed
    /// </summary>
    public class CommandExecutedEventArgs : EventArgs
    {

        /// <summary>
        /// Id of the journal entry or 0 if journaling is disabled
        /// </summary>
        public readonly ulong JournalEntryId;

        /// <summary>
        /// The command object that was executed
        /// </summary>
        public readonly Command Command;

        /// <summary>
        /// Point in time when execution began
        /// </summary>
        public readonly DateTime StartTime;

        /// <summary>
        /// Amount of time taken to execute the command, excluding 
        /// </summary>
        public readonly TimeSpan Duration;

        /// <summary>
        /// Domain events captured during execution
        /// </summary>
        public readonly IEvent[] Events;  

        public CommandExecutedEventArgs(ulong entryId, Command command, DateTime executed, TimeSpan executionTime, IEnumerable<IEvent> events)
        {
            JournalEntryId = entryId;
            Command = command;
            StartTime = executed;
            Duration = executionTime;
            Events = events.ToArray();
        }

    }
}