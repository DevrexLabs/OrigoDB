using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core
{
    [Serializable]
    public class RenameListCommand : Command<TodoModel>
    {
        public readonly string OldName;
        public readonly string NewName;

        public RenameListCommand(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }

        protected override void Prepare(TodoModel model)
        {
            if(model.Lists.Count(list => list.IsNamed(OldName)) == 0)
            {
                throw new InvalidOperationException("No list named " + OldName);
            }

            if (model.Lists.Any(list => list.IsNamed(NewName)))
            {
                throw new InvalidOperationException("Rename would create duplicate lists");
            }
        }

        protected override void Execute(TodoModel model)
        {
            model.Lists.Single(list => list.IsNamed(OldName)).Rename(NewName);
        }
    }
}
