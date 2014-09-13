namespace OrigoDB.Core
{
    /// <summary>
    /// Event filter interface
    /// </summary>
    public interface ISelectEvents
    {
        /// <summary>
        /// Return true if the given event matches the filter
        /// </summary>
        bool Matches(IEvent e);
    }
}