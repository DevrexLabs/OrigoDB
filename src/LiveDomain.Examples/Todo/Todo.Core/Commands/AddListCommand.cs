using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core
{
    [Serializable]
    public class AddListCommand : Command<TodoModel>
    {
        public readonly string Name;

        public AddListCommand(string name)
        {
            Name = name;
        }

        protected override void Prepare(TodoModel model)
        {
            if(model.Lists.Any( list => list.IsNamed(Name)))
            {
                throw new InvalidOperationException("Can't create list with duplicate name");
            }
        }

        protected override void Execute(TodoModel model)
        {
            model.Lists.Add(new TaskList(Name));
        }
    }
}
