using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    [Serializable]
    public class GenericModel<TEntity, TKey> : Model 
        where TEntity : IDomainElement<TKey>
    {
        private IList<TEntity> Entities { get; set; }

        public GenericModel()
        {
            Entities = new List<TEntity>();
        }

        public void Add(TEntity entity)
        {
            Entities.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            Entities.Remove(entity);
        }

        public IEnumerable<TEntity> GetBy(Func<TEntity, bool> predicate)
        {
            return Entities.Where(predicate);
        }

    }
}