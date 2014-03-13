using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// Wrapper class for Lambda queries. 
    /// </summary>
    public class DelegateQuery<TModel, TResult> : Query<TModel, TResult> where TModel : Model
    {
        private readonly Func<TModel, TResult> _lambdaQuery;

        public DelegateQuery(Func<TModel, TResult> lambdaQuery)
        {
            _lambdaQuery = lambdaQuery;
        }

        public override TResult Execute(TModel model)
        {
            return _lambdaQuery.Invoke(model);
        }
        
        public static implicit operator DelegateQuery<TModel, TResult>(Func<TModel, TResult> lamdaQuery)
        {
            return new DelegateQuery<TModel, TResult>(lamdaQuery);
        }
    }
}
