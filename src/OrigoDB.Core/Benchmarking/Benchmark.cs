using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OrigoDB.Core.Benchmarking
{
    public class Benchmark
    {
        /// <summary>
        /// Run continously for this amount of time
        /// </summary>
        public TimeSpan Duration = TimeSpan.MaxValue;

        /// <summary>
        /// The number of threads to use, default is number of CPU cores 
        /// </summary>
        public int? Threads;

        private readonly List<Component> _workload = new List<Component>();

        public void AddComponent(string name, Action action, double weight = 1)
        {
            var source = new Component(name, action, weight);
            _workload.Add(source);
        }

        /// <summary>
        /// Run the benchmark synchronously
        /// </summary>
        public BenchmarkResult Run()
        {
            double[] weights = _workload.Select(c => c.Weight).ToArray().Normalize();

            int threads = Threads ?? Environment.ProcessorCount;
            var seed = new Random().Next(int.MaxValue - 1);

            var timer = new Stopwatch();
            timer.Start();

            //start one task per thread
            var tasks = Enumerable.Range(1, threads)
                .Select(threadIdx => Task.Factory.StartNew(() =>
                {
                    //array based list would mess with timings during reallocation
                    var timings = new LinkedList<Timing>();

                    //We don't want the same sequence on each thread
                    var random = new Random(seed + threadIdx);
                    while (true)
                    {
                        if (timer.Elapsed > Duration) break;

                        //Select a random component with probability
                        //in proportion to its weight
                        var randomIndex = random.WeightedRandom(weights);
                        var component = _workload[randomIndex];

                        var timing = new Timing(component.Name);
                        timing.Time(component.Action, timer);
                        timings.AddLast(timing);
                    }
                    return timings.ToArray();
                })).ToArray();

            // ReSharper disable once CoVariantArrayConversion
            Task.WaitAll(tasks);
            timer.Stop();

            var weightsByKey = _workload.Select((c, idx) => new Component(c.Name, c.Action, weights[idx]))
                .ToDictionary(c => c.Name, c => c.Weight);
            
            return new BenchmarkResult(timer.Elapsed, weightsByKey, tasks.Select(t => t.Result));
        }

        public static TimeSpan TimeThis(int iterations, Action action)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < iterations; i++) action.Invoke();
            stopWatch.Stop();
            return stopWatch.Elapsed;
        }

        public static void Baseline()
        {
            const int iterations = 20000;
            const int messageSize = 300;

            var config = new EngineConfiguration(Guid.NewGuid().ToString());
            var engine = Engine.LoadOrCreate<BenchmarkModel>(config);
            TimeSpan duration = TimeThis(iterations, () => engine.Execute(new BenchmarkCommand(messageSize)));
            engine.Close();
            Console.WriteLine("{0} iterations, timespan: {1}", iterations, duration);
            Console.WriteLine("TPS: " + iterations / duration.TotalSeconds);            
        }

        private class Component
        {
            /// <summary>
            /// A friendly name used to group
            /// </summary>
            public readonly string Name;

            /// <summary>
            /// The action to invoke repeatedly during the benchmark
            /// </summary>
            public readonly Action Action;

            /// <summary>
            /// Frequency of this component relative the weights of
            /// all the components in the same workload
            /// </summary>
            public readonly double Weight;

            public Component(string name, Action action, double weight = 1.0f)
            {
                Name = name;
                Action = action;
                Weight = weight;
            }
        }
    }
}