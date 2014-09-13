namespace OrigoDB.Core
{
    /// <summary>
    /// Event handler interface
    /// </summary>
    public interface IHandleEvents
    {
        /// <summary>
        /// Handle the event, exceptions will be ignored
        /// </summary>
        /// <param name="event"></param>
        void Handle(IEvent @event);
    }
}