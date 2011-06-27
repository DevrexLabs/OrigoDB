using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    static class Timer
    {
        public static TimeSpan TimeThis(Action action, int times = 1, bool display = true)
        {
            DateTime start = DateTime.Now;
            for (int i = 0; i < times; i++)
            {
                action.Invoke();
            }

            TimeSpan duration = DateTime.Now - start;
            if (display) Console.WriteLine(duration);
            return duration;
        }
        
    }
}
