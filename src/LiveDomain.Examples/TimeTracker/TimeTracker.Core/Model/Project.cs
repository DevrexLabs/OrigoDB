using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core
{
    [Serializable]
	public class Project
	{
        [Range(0, Int32.MaxValue)]
        public int Id { get; private set; }
        [Required]
        public string Name { get; set; }        
        public string Description { get; set; }

        public User Owner { get; set; }
        public List<Assignment> Members { get; protected set; }
        public Client Client { get; set; }

        private Dictionary<String, Task> _tasks;	
        public IEnumerable<Task> Tasks 
        { 
            get 
            {
                return _tasks.Values.OrderBy(t => t.Name); 
            }
        }

        public Project(int id, Client client) 
            : this(id, String.Empty, String.Empty, client) { }

		public Project(int id, string name, string description, Client client)
		{
            this.Id = id;
			this.Name = name;
            this.Description = description;
            this.Client = client;
			Members = new List<Assignment>();
			_tasks = new Dictionary<string, Task>();
		}

		public void AddTask(Task task)
		{
			if (_tasks.ContainsKey(task.Name)) throw new InvalidOperationException("Task name already exists");
			task.Project = this;
			_tasks.Add(task.Name, task);
		}
	}
}
