using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// Wrapper class for Lambda queries. 
    /// </summary>
    public class DelegateQuery<M, T> : Query<M, T> where M : Model
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
