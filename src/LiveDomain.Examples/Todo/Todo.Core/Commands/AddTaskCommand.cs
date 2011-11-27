using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core
{
    [Serializable]
    public class AddTaskCommand : Command<TodoModel>
    {

        public readonly Task Task;
        public readonly string ListName;


        protected override void Prepare(TodoModel model)
        {
            if (!model.Lists.Any(list => list.IsNamed(ListName)))
            {
                throw new InvalidOperationException("No list named " + ListName);
            }
        }

        protected override void Execute(TodoModel model)
        {
            model.Lists.Single(list => list.IsNamed(ListName)).Tasks.Add(Task);
        }

        public AddTaskCommand(Task task, string listName)
        {
            Task = task;
            ListName = listName;
        }
    }
}
