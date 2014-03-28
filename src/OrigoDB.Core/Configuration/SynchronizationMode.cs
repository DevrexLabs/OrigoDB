namespace OrigoDB.Core
{

    /// <summary>
    /// Engine chooses Locking strategy based on this setting
    /// </summary>
    public enum SynchronizationMode
    {

        /// <summary>
        /// Allow multiple queries or a single command
        /// </summary>
        ReadWrite,

        /// <summary>
        /// Allow any access, thread safety is controlled by client code
        /// </summary>
        None,

        /// <summary>
        /// Allow access to one thread at a time for either reading or writing
        /// </summary>
        Exclusive,

        /// <summary>
        /// Custom implementation of ISynchronizer is used
        /// </summary>
        Custom
    }
}
