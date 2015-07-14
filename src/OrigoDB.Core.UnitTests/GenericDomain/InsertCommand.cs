using System;
using OrigoDB.Core;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    [Serializable]
    public class InsertCommand<TEntity, TKey> : Command<GenericModel<TEntity, TKey>> where TEntity : IDomainElement<TKey>
    {
        private TEntity Entity { get; set; }

        public InsertCommand(TEntity entity)
        {
            Entity = entity;
        }

        public override void Execute(GenericModel<TEntity, TKey> model)
        {
            model.Add(Entity);
        }
    }
}