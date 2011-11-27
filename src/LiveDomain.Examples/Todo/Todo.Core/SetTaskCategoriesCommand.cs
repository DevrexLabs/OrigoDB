using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core
{
    [Serializable]
    public class SetTaskCategoriesCommand : Command<TodoModel>
    {
        public readonly Guid TaskId;
        public readonly string[] CategoryNames;

        [NonSerialized]
        Task _task;

        protected override void Prepare(TodoModel model)
        {
            _task = model.Lists.SelectMany(list => list.Tasks).Single(task => task.Id == TaskId);
        }

        protected override void Execute(TodoModel model)
        {
            _task.Categories.Clear();
            foreach (Category category in model.Categories)
            {
                category.RemoveTask(_task);
            }

            foreach (string categoryName in CategoryNames)
            {
                Category category = model.Categories.SingleOrDefault(c => c.IsNamed(categoryName));
                if (category == null)
                {
                    category = new Category(categoryName);
                    model.Categories.Add(category);
                }
                category.AddTask(_task);
                _task.Categories.Add(category);
            }
        }

        public SetTaskCategoriesCommand(Guid taskId, string[] categoryNames)
        {
            TaskId = taskId;
            CategoryNames = categoryNames;
        }
    }
}
