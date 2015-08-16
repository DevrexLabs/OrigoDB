using System.Runtime.Serialization;

namespace OrigoDB.Core
{
    public class CloneStrategy
    {
        private readonly Strategy _strategy;

        /// <summary>
        /// The formatter used to clone
        /// </summary>
        private IFormatter _formatter;

        /// <summary>
        /// Apply the strategy to returnvalues. The subject will either be cloned or not depending on the specific strategy chosen.
        /// </summary>
        /// <param name="subject">The thing which might be cloned</param>
        /// <param name="operation">The command or query which produced the subject, or null when n/a</param>
        internal void Apply(ref object subject, IOperationWithResult operation)
        {
            _strategy.Invoke(ref subject, operation, _formatter);
        }

        /// <summary>
        /// Apply to a command
        /// </summary>
        /// <param name="command"></param>
        internal void Apply(ref Command command)
        {
            object subject = command;
            Apply(ref subject, null);
        }


        /// <summary>
        /// Inject the formatter to clone with.
        /// </summary>
        internal void SetFormatter(IFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// Clone unless the subject is known to guarantee isolation
        /// </summary>
        public static CloneStrategy Auto()
        {
            return new CloneStrategy(AutoImpl);   
        }

        /// <summary>
        /// Always clone
        /// </summary>
        public static CloneStrategy Yes()
        {
            return new CloneStrategy(Clone);
        }

        /// <summary>
        /// Never clone
        /// </summary>
        public static CloneStrategy No()
        {
            return _no;
        }

        private readonly static CloneStrategy _no = new CloneStrategy(delegate{});

        //refs not supported with Func/Action
        private delegate void Strategy(ref object subject, IOperationWithResult operation, IFormatter formatter);

        private CloneStrategy(Strategy strategy)
        {
            _strategy = strategy;
        }

        private static void Clone(ref object @object, IOperationWithResult operation, IFormatter formatter)
        {
            @object = formatter.Clone(@object);
        }

        private static void AutoImpl(ref object @object, IOperationWithResult operation, IFormatter formatter)
        {
            if (! IsSafe(@object, operation)) @object = formatter.Clone(@object);
        }

        private static bool IsSafe(object subject, IOperationWithResult operation)
        {
            return (operation != null && operation.ResultIsIsolated)
                   || subject.GetType().IsIsolated();
        }
    }
}
