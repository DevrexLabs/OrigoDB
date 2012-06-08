using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core.Commands
{
    [Serializable]
    public class SetCompletedCommand : Command<TodoModel>
    {
        private DateTime _completeDate;
        private Guid _taskId;


        public SetCompletedCommand(Guid taskId, DateTime completeDate)
        {
            _taskId = taskId;
            _completeDate = completeDate;
        }

        #region Overrides of Command<TodoModel>

        protected override void Execute(TodoModel model)
        {
            var task = model.Lists.SelectMany(l => l.Tasks).Single(t => t.Id == _taskId);
            task.Completed = _completeDate;
        }

        #endregion
    }
}
