using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    [Serializable]
    public class GetByQuery<TEntity, TKey> : 
        Query<GenericModel<TEntity, TKey>, IEnumerable<TEntity>> 
        where TEntity : IDomainElement<TKey>
    {
        private Func<TEntity, bool> Predicate { get; set; }

        public GetByQuery(Func<TEntity, bool> predicate)
        {
            Predicate = predicate;
        }

        public override IEnumerable<TEntity> Execute(GenericModel<TEntity, TKey> model)
        {
            return model.GetBy(Predicate).ToList();
        }
    }
}