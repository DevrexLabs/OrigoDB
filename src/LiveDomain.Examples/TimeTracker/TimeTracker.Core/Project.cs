using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    [Serializable]
	public class Project
	{

		private Dictionary<String,Task> _tasks;

		public string Name { get; private set; }
        public string Description { get; set; }
		public User Owner { get; set; }

        public List<Assignment> Members { get; protected set; }
		public IEnumerable<Task> Tasks { get { return _tasks.Values.OrderBy(t => t.Name); } }


		public Project(string name, User owner)
		{
			this.Name = name;
			this.Owner = owner;
			Members = new List<Assignment>();
			_tasks = new Dictionary<string, Task>(StringComparer.InvariantCultureIgnoreCase);
		}

		public void Assign(User person, String role, decimal hourlyRate)
		{
			//var assignment = new Assignment { Project = this, Person = person, Role = role, HourlyRate = hourlyRate};
			//Members.Add(assignment);
		}

		public void AddTask(Task task)
		{
			if (_tasks.ContainsKey(task.Name)) throw new InvalidOperationException("Task name already exists");
			task.Project = this;
			_tasks.Add(task.Name, task);
		}
	}
}
