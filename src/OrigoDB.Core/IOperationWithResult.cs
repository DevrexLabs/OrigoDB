namespace OrigoDB.Core
{

    /// <summary>
    /// Shared by Query and Command of M,R (commands that return results)
    /// </summary>
    public interface IOperationWithResult
    {
        /// <summary>
        /// True if results are safe to return to client, default is false.
        /// Set to true if your command/query implementation guarantees no references to
        /// mutable objects within the model are returned.
        /// </summary>
        bool ResultIsIsolated { get;}
    }
}