using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{

    public class Execution
    {
        [ThreadStatic]
        private static Execution _current;

        public Execution() :this(DateTime.Now) { }

        public Execution(DateTime now)
        {
            Now = now;
            Events = new List<IEvent>(20);
        }

        /// <summary>
        /// Time according to the system clock just prior to writing to the journal. 
        /// Will have exact same value during restore. Use instead of DateTime.Now
        /// </summary>
        public DateTime Now { get; protected internal set; }

        /// <summary>
        /// Sequence of events produced by the current execution. 
        /// Add events here and subscribe to Engine.CommandExecuted to collect events.
        /// </summary>
        public IList<IEvent> Events { get; protected internal set; }

        /// <summary>
        /// The current execution context. Only accesible from within the command/query execution pipeline.
        /// Otherwise it is null.
        /// </summary>
        public static Execution Current
        {
            get { return _current; }
            set { _current = value; }
        }

        internal static Execution Begin()
        {
            Current = new Execution();
            return Current;
        }
    }
}
