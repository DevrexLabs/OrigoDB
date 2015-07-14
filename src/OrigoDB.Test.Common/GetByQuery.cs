using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core;

namespace OrigoDB.Test.Common
{
    [Serializable]
    public class GetByQuery<TEntity> : Query<GenericModel<TEntity>, IEnumerable<TEntity>> where TEntity : Entity
    {
        private Func<TEntity, bool> Predicate { get; set; }

        public GetByQuery(Func<TEntity, bool> predicate)
        {
            Predicate = predicate;
        }
        public override IEnumerable<TEntity> Execute(GenericModel<TEntity> model)
        {
            return model.Entities.Where(Predicate).ToList();
        }
    }
}