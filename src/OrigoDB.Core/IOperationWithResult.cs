namespace OrigoDB.Core
{

    /// <summary>
    /// Shared by Query and Commands that return values
    /// </summary>
    public interface IOperationWithResult
    {
        /// <summary>
        /// True if results are safe to return to client.
        /// Set to true if your command/query implementation guarantees no references to
        /// mutable objects within the model are returned.
        /// </summary>
        bool? ResultIsIsolated{ get; set; }
    }
}