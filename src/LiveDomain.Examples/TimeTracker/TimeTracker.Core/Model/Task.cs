using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    [Serializable]
	public class Task
	{
		public String Name { get; set; }
		public String Description { get; set; }
		public Project Project { get; set; }
		public User AssignedTo { get; set; }

		public List<WorkItem> WorkUnits { get; set; }
		public TimeSpan EstimatedTotalTime { get; set; }
        public DateTime DueBy { get; set; }
        public DateTime StartBy { get; set; }
		public TaskState State { get; set; }

        public TimeSpan TimeConsumed()
        {
            var result = TimeSpan.FromDays(0);
            foreach (var workUnit in WorkUnits)
            {
                result += workUnit.Duration;
            }
            return result;
        }

		public Task(string name, string description, TimeSpan estimatedTotalTime)
		{
			Name = name;
			Description = description;
			WorkUnits = new List<WorkItem>();
			EstimatedTotalTime = estimatedTotalTime;
		}
	}
}
