using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Todo.Core
{
    [Serializable]
    public class TaskList
    {

        public string Name { get; private set; }
        public List<Task> Tasks { get; private set; }

        public void Rename(string newName)
        {
            Name = newName;
        }

        public TaskList(string name)
        {
            Tasks = new List<Task>();
            Name = name;
        }

        public bool IsNamed(string name)
        {
            return String.Compare(Name, name, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
