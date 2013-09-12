using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    [Serializable]
    public class ChildModelQuery<M, C, T> : Query<M, T>
        where C : Model
        where M : Model
    {
        private Query<C, T> _query;

        public ChildModelQuery(Query<C, T> query)
        {
            _query = query;
        }

        protected override T Execute(M model)
        {
            return (T) _query.ExecuteStub(model.ChildFor<C>());
        }
    }
}
