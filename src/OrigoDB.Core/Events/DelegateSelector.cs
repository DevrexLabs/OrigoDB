using System;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
    /// <summary>
    /// ISelectEvents adapter for generic Func
    /// </summary>
    public class DelegateSelector : ISelectEvents
    {

        /// <summary>
        /// A selector which matches any event
        /// </summary>
        public static readonly ISelectEvents Any;

        /// <summary>
        /// Initializes the static field Any
        /// </summary>
        static DelegateSelector()
        {
            Any = new DelegateSelector(_ => true);
        }
        
        readonly Func<IEvent, bool> _selector;

        /// <summary>
        /// Constructor accepting a non null filter
        /// </summary>
        public DelegateSelector(Func<IEvent,bool> selector)
        {
            Ensure.NotNull(selector, "selector");
            _selector = selector;
        }
        
        /// <summary>
        /// Executes the wrapped lambda
        /// </summary>
        public bool Matches(IEvent e)
        {
            return _selector.Invoke(e);
        }
    }
}