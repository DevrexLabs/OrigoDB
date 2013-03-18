using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{

    public abstract class RolloverStrategy
    {
        public abstract bool Rollover(long bytesWritten, long entriesWritten);
    }

    public class CompositeRolloverStrategy : RolloverStrategy
    {
        private List<RolloverStrategy> _childStrategies;

        public CompositeRolloverStrategy()
        {
            _childStrategies = new List<RolloverStrategy>();
        }

        public void AddStrategy(RolloverStrategy strategy)
        {
            _childStrategies.Add(strategy);
        }

        public override bool Rollover(long bytesWritten, long entriesWritten)
        {
            return _childStrategies.Any(strategy => strategy.Rollover(bytesWritten, entriesWritten));
        }
    }


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
