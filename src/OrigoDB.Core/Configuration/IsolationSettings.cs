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
        /// Applies to results returned by commands and queries
        /// </summary>
        public CloneStrategy ReturnValues { get; set; }

        public IsolationSettings()
        {
            Commands = CloneStrategy.Auto();
            ReturnValues = CloneStrategy.Auto();
        }

        public static IsolationSettings ForImmutability()
        {
            return new IsolationSettings()
            {
                Commands = CloneStrategy.No(),
                ReturnValues = CloneStrategy.No()
            };
        }
    }
}