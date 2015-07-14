namespace OrigoDB.Core
{
    public enum PersistenceMode
    {
        /// <summary>
        /// Default, write each command to the journal before applying to the model
        /// </summary>
        Journaling,

        /// <summary>
        /// No journaling, write a snapshot for each command.
        /// </summary>
        SnapshotPerTransaction,

        /// <summary>
        /// Take a snapshot on demand
        /// </summary>
        ManualSnapshots
    }
}