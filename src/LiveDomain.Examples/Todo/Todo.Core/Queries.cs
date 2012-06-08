using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core
{
    [Serializable]
    public class GetListNames : Query<TodoModel,string[]>
    {
        #region Overrides of Query<TodoModel,string[]>

        protected override string[] Execute(TodoModel m)
        {
            return m.Lists.Select(list => list.Name).ToArray();
        }

        #endregion
    }

    [Serializable]
    public class GetTasksByListName : Query<TodoModel, TaskInfo[]>
    {
        private string _listName;

        public GetTasksByListName(string listName)
        {
            _listName = listName;
        }

        #region Overrides of Query<TodoModel,string[]>

        protected override TaskInfo[] Execute(TodoModel m)
        {
            return m.Lists.Single(list => list.IsNamed(_listName)).Tasks.Select(task => new TaskInfo(task)).ToArray();
        }

        #endregion
    }
}
