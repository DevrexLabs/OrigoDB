using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core.Benchmarking
{
    public class BenchmarkResult
    {
        /// <summary>
        /// Sequence of timings produced by each thread
        /// </summary>
        public readonly List<Timing[]> Timings;

        /// <summary>
        /// Weights by component Key
        /// </summary>
        public readonly Dictionary<string, double> Weights;
 
        /// <summary>
        /// Total time consumed by the benchmark, including Task setup/cleanup
        /// </summary>
        public readonly TimeSpan Elapsed;

        public BenchmarkResult(TimeSpan elapsed, Dictionary<string,double> weights, IEnumerable<Timing[]> timings)
        {
            Weights = weights;
            Elapsed = elapsed;
            Timings = timings.ToList();
        }

        public Dictionary<string, Statistics> StatisticsByKey(Func<Timing,string> groupSelector = null)
        {
            groupSelector = groupSelector ?? (timing => timing.Key);
            return Timings.SelectMany(_ => _)
                .GroupBy(groupSelector)
                .ToDictionary(
                    g => g.Key,
                    g => Statistics.Create(g.Select(m => m.Duration.TotalMilliseconds)));
        }

        public Statistics TotalStatistics()
        {
            var durations = Timings.SelectMany(_ => _).Select(m => m.Duration.TotalMilliseconds);
            return Statistics.Create(durations);
        }
    }
}