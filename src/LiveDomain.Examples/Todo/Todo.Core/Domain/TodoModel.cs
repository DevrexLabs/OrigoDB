 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core
{
    [Serializable]
    public class TodoModel : Model
    {
        public List<TaskList> Lists { get; private set; }
        public HashSet<Category> Categories { get; private set; }

        public TodoModel()
        {
            Lists = new List<TaskList>();
            Categories = new HashSet<Category>();
        }
    }
}
