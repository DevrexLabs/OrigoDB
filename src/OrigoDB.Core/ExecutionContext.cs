using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    public class ExecutionContext
    {
        [ThreadStatic]
        private static ExecutionContext _current;


        public ExecutionContext()
            :this(DateTime.Now)
        {
            
        }

        public ExecutionContext(DateTime timestamp)
        {
            Timestamp = timestamp;
        }

        /// <summary>
        /// Time according to the system clock just prior to writing to the journal. 
        /// Will have exact same value during restore. Use instead of DateTime.Now
        /// </summary>
        public DateTime Timestamp { get; protected internal set; }
        
        /// <summary>
        /// List of events added with AddEvent()
        /// </summary>
        protected internal List<IEvent> Events = new List<IEvent>();

        /// <summary>
        /// Add a domain event to the context. Subscribe to Engine.CommandExecuted to collect events.
        /// </summary>
        public void AddEvent(IEvent @event)
        {
            Events.Add(@event);
        }


        /// <summary>
        /// The current context. Only accesible from within the command/query execution pipeline.
        /// Otherwise it is null.
        /// </summary>
        public static ExecutionContext Current
        {
            get { return _current; }
            set { _current = value; }
        }

        internal static ExecutionContext Begin()
        {
            Current = new ExecutionContext();
            return Current;
        }
    }
}
