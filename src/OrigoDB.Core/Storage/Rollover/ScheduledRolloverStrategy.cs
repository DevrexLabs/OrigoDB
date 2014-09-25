using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// Rolls over at given intervals of time, default is every midnight
    /// </summary>
    public class ScheduledRolloverStrategy : RolloverStrategy
    {
        /// <summary>
        /// Rollover when this period of time has elapsed
        /// </summary>
        public readonly TimeSpan Interval;
        
        /// <summary>
        /// Initial time or last rollover
        /// </summary>
        public DateTime ReferenceTime;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">Reference time, defaults to previous midnight</param>
        /// <param name="interval">Defaults to 24 hours</param>
        public ScheduledRolloverStrategy(DateTime? initial = null, TimeSpan? interval = null)
        {
            Interval = interval ?? TimeSpan.FromHours(24);
            ReferenceTime = initial ?? DateTime.Today;
        }

        /// <summary>
        /// Checks if interval has elapsed and updates reference time if so
        /// </summary>
        /// <param name="bytesWritten">unused</param>
        /// <param name="entriesWritten">unused</param>
        /// <returns></returns>
        public override bool Rollover(long bytesWritten, long entriesWritten)
        {
            var intervalElapsed = DateTime.Now > ReferenceTime + Interval;
            if (intervalElapsed) ReferenceTime += Interval;
            return intervalElapsed;
        }
    }
}