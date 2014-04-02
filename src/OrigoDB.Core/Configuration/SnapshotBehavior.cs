namespace OrigoDB.Core
{
    public enum SnapshotBehavior
    {

        /// <summary>
        /// No automatic snapshots
        /// </summary>
        None,

        /// <summary>
        /// Take a snapshot when engine is loaded
        /// </summary>
        AfterRestore,

        /// <summary>
        /// Take a snaphot when the engine is shutting down
        /// </summary>
        OnShutdown
    }
}
