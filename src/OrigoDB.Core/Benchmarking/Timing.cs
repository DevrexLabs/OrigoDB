using System;
using System.Diagnostics;

namespace OrigoDB.Core.Benchmarking
{
    public class Timing
    {
        public readonly string Key;

        public long StartTick { get; private set; }
        
        public TimeSpan Duration { get; private set; }
        
        public bool Threw { get; private set; }

        public Timing(string key)
        {
            Key = key;
        }

        public void Time(Action action, Stopwatch timer)
        {
            try
            {
                StartTick = timer.ElapsedTicks;
                action.Invoke();
            }
            catch
            {
                Threw = true;
            }
            Duration = timer.Elapsed;
        }
    }
}