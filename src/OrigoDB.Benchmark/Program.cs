using System;
using OrigoDB.Core;
using OrigoDB.Core.Configuration;
using System.Diagnostics;

namespace OrigoDB.Benchmark
{

    [Serializable]
    public class BenchmarkModel : Model
    {
        public int CommandsExecuted;
        public long BytesWritten;
    }

    [Serializable]
    public class Message : Command<BenchmarkModel>
    {
        public readonly byte[] Payload;

        public Message(int size)
        {
            Payload = new byte[size];
        }

        public override void Execute(BenchmarkModel model)
        {
            model.CommandsExecuted++;
            model.BytesWritten += Payload.Length;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int iterations = 20000;
            const int messageSize = 3000;
            
            var config = new EngineConfiguration(Guid.NewGuid().ToString());
            config.PacketOptions = PacketOptions.Compressed;
            config.Kernel = Kernels.Optimistic;
            var engine = Engine.LoadOrCreate<BenchmarkModel>(config);
            TimeThis(iterations, () => engine.Execute(new Message(messageSize)));
            engine.Close();
            Console.ReadLine();
        }

        static void TimeThis(int iterations, Action action)
        {
            Console.WriteLine("{0} iterations..", iterations);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                action.Invoke();
            }
            stopWatch.Stop();
            long millisElapsed = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Elapsed: {0} ms", millisElapsed);
            long invocationsPerSecond = iterations*1000/millisElapsed;
            Console.WriteLine("Invocations per second: {0}", invocationsPerSecond);
        }

        static void TimeThese(int iterations, params Action[] actions)
        {
            
        }
    }
}
