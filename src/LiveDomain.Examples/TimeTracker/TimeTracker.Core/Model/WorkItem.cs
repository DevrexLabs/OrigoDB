using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    /// <summary>
    /// A unit of work performed against a task
    /// </summary>
    [Serializable]
	public class WorkItem
	{
		public Task Task { get; set; }
		public DateTime Started { get; set; }
		public TimeSpan Duration { get; set; }
		public Assignment Assignment { get; set; }
		
        /// <summary>
        /// A short description visible to the client
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Notes visible internally
        /// </summary>
        public string Notes { get; set; }
	}
}
