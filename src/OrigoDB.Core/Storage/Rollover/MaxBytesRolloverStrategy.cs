namespace OrigoDB.Core
{
    /// <summary>
    /// Signals rollover when number of bytes written exceeds a given limit
    /// </summary>
    public class MaxBytesRolloverStrategy : RolloverStrategy
    {
        public readonly long MaxBytes;
        public const long DefaultMaxBytes = 1024 * 1024 * 4;

        public MaxBytesRolloverStrategy(long maxBytes = DefaultMaxBytes)
        {
            MaxBytes = maxBytes;
        }

        public override bool Rollover(long bytesWritten, long entriesWritten)
        {
            return bytesWritten >= MaxBytes;
        }
    }
}