using System;

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
        public readonly Command Command;
        public readonly DateTime StartTime;
        public readonly TimeSpan Duration;

        public CommandExecutedEventArgs(ulong entryId, Command command, DateTime executed, TimeSpan executionTime)
        {
            JournalEntryId = entryId;
            Command = command;
            StartTime = executed;
            Duration = executionTime;
        }

    }
}