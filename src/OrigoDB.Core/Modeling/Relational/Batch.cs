using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Add an entity to be inserted.
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(IEntity entity)
        {
            EnsureUnique(entity);
            Inserts.Add(entity);
        }

        /// <summary>
        /// Add an entity to replace the entity with the same type, key and version
        /// </summary>
        /// <param name="entity"></param>
        public void Update(IEntity entity)
        {
            EnsureUnique(entity);
            Updates.Add(entity);
        }

        /// <summary>
        /// Add an entity or (EntityKey) to be deleted when the batch is executed. 
        /// Entity must exist and have a matching version
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(IEntity entity)
        {
            EnsureUnique(entity);
            if (entity.GetType() != typeof(EntityKey)) entity = new EntityKey(entity);
            Deletes.Add(entity);
        }

        private void EnsureUnique(IEntity entity)
        {
            var id = entity.Id;
            if (_unique.Contains(id)) throw new ArgumentException("Duplicate id: " + id );
            _unique.Add(id);
        }

        /// <summary>
        /// Return the set of entity types present in this batch
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> Types()
        {
            return
                Deletes.Cast<EntityKey>()
                    .Select(d => d.Type)
                    .Concat(Inserts.Concat(Updates).Select(ui => ui.GetType()))
                    .Distinct();
        }
    }
}