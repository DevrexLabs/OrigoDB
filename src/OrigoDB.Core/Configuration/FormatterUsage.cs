namespace OrigoDB.Core
{

    /// <summary>
    /// Identifier for different usages of formatters.
    /// </summary>
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
        /// Format journal entries (commands written to the journal)
        /// </summary>
        Journal,

        /// <summary>
        /// Format return values from commands and queries
        /// </summary>
        Results,

        /// <summary>
        /// Format messages, (including commands and queries) between client and server and between servers
        /// </summary>
        Messages
    }
}