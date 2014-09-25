namespace OrigoDB.Core
{
    /// <summary>
    /// Signals rollover when number of entries exceeds a given limit
    /// </summary>
    public class MaxEntriesRolloverStrategy : RolloverStrategy
    {
        public readonly long MaxEntries;
        public const long DefaultMaxEntries = 1000;

        public MaxEntriesRolloverStrategy(long maxEntries = DefaultMaxEntries)
        {
            MaxEntries = maxEntries;
        }

        public override bool Rollover(long bytesWritten, long entriesWritten)
        {
            return entriesWritten >= MaxEntries;
        }
    }
}