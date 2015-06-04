using System;
using System.Linq;

namespace OrigoDB.Core.Benchmarking
{
    internal static class RandomExtensions
    {
        /// <summary>
        /// Select a random weight with probability in proportion to 
        /// </summary>
        /// <param name="random"></param>
        /// <param name="weights">sum of weights must be = 1</param>
        /// <returns>zero-based index of the selected weight</returns>
        public static int WeightedRandom(this Random random, double[] weights)
        {
            while (true)
            {
                double r = random.NextDouble();
                for (int i = 0; i < weights.Length; i++)
                {
                    r -= weights[i];
                    if (r < 0) return i;
                }
            }
        }

        /// <summary>
        /// Scale each value proportionately so all weights add up to 1.0
        /// </summary>

        public static double[] Normalize(this double[] values)
        {
            double sum = values.Sum();
            return values.Select(val => val/sum).ToArray();
        }
    }
}