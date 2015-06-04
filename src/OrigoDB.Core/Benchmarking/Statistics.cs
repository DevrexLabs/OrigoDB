using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core.Benchmarking
{
    public class Statistics
    {
        private readonly double[] _values;

        public long Count { get; private set; }

        public double Sum { get; private set; }

        public double MeanAverage { get; private set; }

        public double Min { get; private set; }

        public double Max { get; private set; }



        public static Statistics Create(IEnumerable<double> items)
        {
            var stats = new Statistics(items.ToArray());
            stats.Calculate();
            return stats;
        } 

        private void Calculate()
        {
            Array.Sort(_values); 
            Count = _values.Length;
            Sum = _values.Sum();
            Min = _values[0];
            Max = _values[_values.Length-1];
            MeanAverage = Sum/Count;
        }

        /// <summary>
        /// Percentile using Nearest Rank method
        /// </summary>
        /// <param name="percentile">must be > 0 but not > 100</param>
        public double Percentile(double percentile)
        {
            if (percentile <= 0 || percentile > 100) 
                throw new ArgumentOutOfRangeException("percentile", percentile, "Percentile must be > 0 and <= 100");
            int rank = (int) Math.Ceiling(percentile/100*Count);
            return _values[rank - 1];
        }

        public int[] Histogram(int numSlices)
        {
            var histogram = new int[numSlices];

            double width = (Max - Min) /numSlices;
            double threshold = Min + width;
            int idx = 0;
            foreach (var value in _values)
            {
                while (value >= threshold && idx < numSlices - 1)
                {
                    idx++;
                    threshold += width;
                }
                histogram[idx]++;
            }
            return histogram;
        }

        private Statistics(double[] values)
        {
            _values = values;
        }
    }
}