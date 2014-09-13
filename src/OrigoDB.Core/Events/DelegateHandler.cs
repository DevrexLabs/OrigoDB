using System;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
    /// <summary>
    /// IHandleEvents wrapper for generic Action
    /// </summary>
    public class DelegateHandler : IHandleEvents
    {
        readonly Action<IEvent> _handler;

        /// <summary>
        /// Constructor requiring a non null Action
        /// </summary>
        public DelegateHandler(Action<IEvent> handler)
        {
            Ensure.NotNull(handler, "handler");
            _handler = handler;
        }

        /// <summary>
        /// Handle the event by invoking the wrapped Action
        /// </summary>
        public void Handle(IEvent @event)
        {
            _handler.Invoke(@event);    
        }

        /// <summary>
        /// Identity is based on the wrapped Action. Override to ensure we can unsubscribe a previously registered Action
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is DelegateHandler && (obj as DelegateHandler)._handler == _handler;
        }

        /// <summary>
        /// Override to ensure we can unsubscribe a previously registered Action
        /// </summary>
        public override int GetHashCode()
        {
            return _handler.GetHashCode();
        }
    }
}