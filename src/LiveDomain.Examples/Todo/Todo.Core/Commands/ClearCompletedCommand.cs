using System;
using System.Linq;
using LiveDomain.Core;

namespace Todo.Core.Commands
{
    [Serializable]
    public class ClearCompletedCommand : Command<TodoModel>
    {
        private Guid _taskId;


        public ClearCompletedCommand(Guid taskId)
        {
            _taskId = taskId;
        }

        #region Overrides of Command<TodoModel>

        protected override void Execute(TodoModel model)
        {
            var task = model.Lists.SelectMany(l => l.Tasks).Single(t => t.Id == _taskId);
            task.Completed = null;
        }

        #endregion
    }
}