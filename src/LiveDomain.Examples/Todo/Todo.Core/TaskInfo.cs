using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Todo.Core
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TaskInfo
    {
        public readonly Guid Id;
        public readonly string Title;
        public readonly string Description;
        public readonly DateTime? DueBy;
        public readonly DateTime? Completed;

        public TaskInfo(Task task)
        {
            Id = task.Id;
            Title = task.Title;
            Description = task.Description;
            DueBy = task.DueBy;
            Completed = task.Completed;
        }
    }
}
