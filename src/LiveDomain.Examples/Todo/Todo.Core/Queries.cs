using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Todo.Core
{
    public static class Queries
    {

        public static Func<TodoModel, IEnumerable<TaskInfo>> GetTasksByListName(string listName)
        {
            return db => db.Lists
                            .Single(list => list.IsNamed(listName))
                            .Tasks
                            .Select(task => new TaskInfo(task)).ToArray();
        }
    }
}
