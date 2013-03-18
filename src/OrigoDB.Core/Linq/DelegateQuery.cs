using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    /// <summary>
    /// Wrapper class for Lambda queries. 
    /// </summary>
    internal class DelegateQuery<M, T> : Query<M, T> where M : Model
    {
        private readonly Func<M, T> _lambdaQuery;

        public DelegateQuery(Func<M, T> lambdaQuery)
        {
            _lambdaQuery = lambdaQuery;
        }

        protected override T Execute(M db)
        {
            return _lambdaQuery.Invoke(db);
        }
    }
}
