namespace OrigoDB.Core
{
    [Immutable]
    public class IsolationSettings
    {
        /// <summary>
        /// Applies to commands, default is Auto
        /// </summary>
        public CloneStrategy Commands { get; set; }

        /// <summary>
        /// Applies to results returned by commands and queries, default is Auto
        /// </summary>
        public CloneStrategy ReturnValues { get; set; }

        public IsolationSettings()
        {
            Commands = CloneStrategy.Heuristic;
            ReturnValues = CloneStrategy.Heuristic;
        }

        public static IsolationSettings ForImmutability()
        {
            return new IsolationSettings()
            {
                Commands = CloneStrategy.Never,
                ReturnValues = CloneStrategy.Never
            };
        }
    }
}