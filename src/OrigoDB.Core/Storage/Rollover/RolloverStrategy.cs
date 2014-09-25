namespace OrigoDB.Core
{

    /// <summary>
    /// RolloverStrategy decides when it's time to create 
    /// a new journal segment.
    /// </summary>
    public abstract class RolloverStrategy
    {
        public abstract bool Rollover(long bytesWritten, long entriesWritten);
    }
}
