using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core
{
    /// <summary>
    /// Combines multiple strategies. Signals rollover if any child signals.
    /// </summary>
    public class CompositeRolloverStrategy : RolloverStrategy
    {
        private readonly List<RolloverStrategy> _childStrategies;

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
}