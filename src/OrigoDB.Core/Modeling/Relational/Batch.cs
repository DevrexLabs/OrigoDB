using System;
using System.Collections.Generic;

namespace OrigoDB.Core.Modeling.Relational
{
    /// <summary>
    /// A batch consists of multiple inserts, updates and deletes. A batch will be
    /// processed as a single ACID transaction.
    /// </summary>
    [Serializable]
    public class Batch
    {
        internal readonly List<IEntity> Inserts = new List<IEntity>();
        internal readonly List<IEntity> Updates = new List<IEntity>();
        internal readonly List<IEntity> Deletes = new List<IEntity>();

        [NonSerialized]
        private readonly ISet<Guid> _unique = new HashSet<Guid>(); 

        public void Insert(IEntity entity)
        {
            EnsureUnique(entity);
            Inserts.Add(entity);
        }

        public void Update(IEntity entity)
        {
            EnsureUnique(entity);
            Updates.Add(entity);
        }

        public void Delete(IEntity entity)
        {
            EnsureUnique(entity);
            if (entity.GetType() != typeof(EntityKey)) entity = new EntityKey(entity);
            Deletes.Add(entity);
        }

        private void EnsureUnique(IEntity entity)
        {
            var id = entity.Id;
            if (_unique.Contains(id)) throw new InvalidOperationException("Duplicate id in transaction");
            _unique.Add(id);
        }
    }
}