using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace Todo.Core.Queries
{
    [Serializable]
    public class GetListNamesQuery : Query<TodoModel,string[]>
    {
        #region Overrides of Query<TodoModel,string[]>

        protected override string[] Execute(TodoModel m)
        {
            return m.Lists.Select(list => list.Name).ToArray();
        }

        #endregion
    }

    [Serializable]
    public class GetTasksByListNameQuery : Query<TodoModel, TaskInfo[]>
    {
        private string _listName;

        public GetTasksByListNameQuery(string listName)
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
