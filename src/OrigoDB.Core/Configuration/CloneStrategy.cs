using System.Runtime.Serialization;

namespace OrigoDB.Core
{
    public abstract class CloneStrategy
    {

        /// <summary>
        /// The formatter used to clone
        /// </summary>
        protected IFormatter Formatter;


        internal abstract void Apply(ref Command command);

        internal abstract void Apply(ref object result, object producer);


        /// <summary>
        /// Inject the formatter to clone with.
        /// </summary>
        internal void SetFormatter(IFormatter formatter)
        {
            Formatter = formatter;
        }

        /// <summary>
        /// Clone unless the subject is known to guarantee isolation
        /// </summary>
        public static CloneStrategy Heuristic
        {
            get
            {
                return new HeuristicCloneStrategy();    
            }
        }

        /// <summary>
        /// Always clone
        /// </summary>
        public static CloneStrategy Always
        {
            get
            {
                return new AlwaysCloneStrategy();
            }
        }

        /// <summary>
        /// Never clone
        /// </summary>
        public static CloneStrategy Never
        {
            get
            {
                return new NeverCloneStrategy();
            }
        }
    }
}
