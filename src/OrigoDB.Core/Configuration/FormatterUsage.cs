namespace OrigoDB.Core
{

    public enum FormatterUsage
    {
        /// <summary>
        /// Used when no specific usage is present 
        /// </summary>
        Default,

        /// <summary>
        /// Format snapshots
        /// </summary>
        Snapshot,

        /// <summary>
        /// Format journal entries
        /// </summary>
        Journal,

        /// <summary>
        /// Format return values from commands and queries
        /// </summary>
        Results,

        /// <summary>
        /// Format messages between client and server and between servers
        /// </summary>
        Messages
    }
}