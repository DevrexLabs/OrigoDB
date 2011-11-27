using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Todo.Core
{
    [Serializable]
    public class Category
    {
        public string Name { get; private set; }
        public HashSet<Task> Tasks { get; private set; }
        
        public Category(string name)
        {
            Name = name;
            Tasks = new HashSet<Task>();
        }

        public void AddTask(Task task)
        {
            if (!Tasks.Contains(task)) Tasks.Add(task);
        }

        public void RemoveTask(Task task)
        {
            if (Tasks.Contains(task)) Tasks.Remove(task);
        }


        internal bool IsNamed(string categoryName)
        {
            return String.Compare(Name, categoryName, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
