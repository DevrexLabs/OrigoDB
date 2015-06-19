using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OrigoDB.Core.Benchmarking;

namespace OrigoDB.Test.NUnit
{
    [TestFixture]
    public class BenchmarkTests
    {

        [Test]
        public void WeightedRandomizationTest()
        {
            //generate some random doubles
            var rnd = new Random();
            double[] weights = Enumerable
                .Repeat(0, rnd.Next(100) + 10)
                .Select(_ => (double)rnd.Next(200) + 1).ToArray();

            //scale them so they sum up to 1
            double[] scaledWeights = weights.Normalize();

            //generate random samples from the weights
            double[] samples = new double[weights.Length];
            for (int i = 0; i < 1000000; i++)
            {
                int r = rnd.WeightedRandom(scaledWeights);
                samples[r]++;
            }

            double[] scaledSamples = samples.Normalize();

            //the generated samples should approach the original distribution

            for (int i = 0; i < weights.Length; i++)
            {
                Assert.AreEqual(scaledSamples[i], scaledWeights[i], 0.01, "failed at " + i);
            }
        }

        private static Action RandomSleep(int minMillis, int maxMillis)
        {
            var rand = new Random();
            return () => Thread.Sleep(minMillis + rand.Next(maxMillis - minMillis));
        }

        [Test]
        public void NormalizedArraySumsUp()
        {
            var random = new Random();
            var doubles = Enumerable
                .Repeat(0, random.Next(100))
                .Select(_ => (double) random.Next())
                .ToArray();

            var normalized = doubles.Normalize();
            Assert.AreEqual(1, normalized.Sum(),0.001);
        }

        [Test]
        public void SmokeTest()
        {
            var target = new Benchmark();
            target.Duration = TimeSpan.FromSeconds(2);
            target.Threads = 4;
            target.AddComponent("Commands", RandomSleep(10, 50));
            target.AddComponent("Queries", RandomSleep(2,5), 10);
            BenchmarkResult result = target.Run();

            Console.WriteLine("Elapsed: " + result.Elapsed);

            var stats = result.StatisticsByKey();
            stats.Add("Totals", result.TotalStatistics());
            foreach (var stat in stats)
            {
                Console.WriteLine(stat.Key);
                if (stat.Key != "Totals") Console.WriteLine("Weight: " + result.Weights[stat.Key]);
                Console.WriteLine("count  : " + stat.Value.Count);
                Console.WriteLine("sum    : " + stat.Value.Sum);
                Console.WriteLine("avg    : " + stat.Value.MeanAverage);
                Console.WriteLine("min    : " + stat.Value.Min);
                Console.WriteLine("max    : " + stat.Value.Max);
                Console.WriteLine("TPS    : " + stat.Value.Count / result.Elapsed.TotalSeconds);
                Console.WriteLine("Percentiles: ");
                foreach (var p in new[]{10,20,30,40,50,60,70,80,90,99})
                {
                    var percentile = stat.Value.Percentile(p);
                    Console.WriteLine("{0}\t{1}", p, percentile);
                }
                Console.WriteLine();
                Console.Write("Histogram: ");
                foreach (var count in stat.Value.Histogram(10))
                {
                    Console.Write(count + ", ");
                }
                Console.WriteLine();
                Assert.AreEqual(stat.Value.Count, stat.Value.Histogram(10).Sum());
                Assert.AreEqual(stat.Value.Count, stat.Value.Histogram(20).Sum());
                Assert.AreEqual(stat.Value.Count, stat.Value.Histogram(30).Sum());

            }
        }

    }
}
