using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// A command is about to be executed
    /// </summary>
    public class CommandExecutingEventArgs : EventArgs
    {
        /// <summary>
        /// The command about to be executed
        /// </summary>
        public readonly Command Command;

        /// <summary>
        /// If set to true by any listener the command will be aborted
        /// </summary>
        public bool Cancel { get; set; }

        public CommandExecutingEventArgs(Command command)
        {
            Command = command;
        }
    }
}