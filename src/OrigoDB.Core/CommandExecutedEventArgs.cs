using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// A command has just executed
    /// </summary>
    public class CommandExecutedEventArgs : EventArgs
    {
        public readonly int JournalEntryId;
        public readonly Command Command;
        public readonly DateTime Executed;
        public readonly TimeSpan ExecutionTime;
        //todo: add more fields, prepare, preparetime

        public CommandExecutedEventArgs(int entryId, Command command, DateTime executed, TimeSpan executionTime)
        {
            JournalEntryId = entryId;
            Command = command;
            Executed = executed;
            ExecutionTime = executionTime;
        }

    }
}