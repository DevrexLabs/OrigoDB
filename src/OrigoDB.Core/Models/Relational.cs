using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core.Models
{
    /// <summary>
    /// Entities must implement this interface
    /// </summary>
    public interface IEntity
    {
        Guid Id { get; set; }
        int Version { get; set; }
    }

    /// <summary>
    /// Derive entity classes from this to inherit default implementation of required fields
    /// </summary>
    [Serializable]
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; }
        public int Version { get; set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        protected Entity(Guid id)
        {
            Id = id;
        }

        public EntityKey ToKey()
        {
            return new EntityKey
            {
                Id = Id,
                Version = Version,
                Type = GetType()
            };
        }
    }

    /// <summary>
    /// Identifies an entity by key, version and type. Used by Delete to avoid passing data fields when id is sufficient.
    /// </summary>
    [Serializable]
    public class EntityKey : Entity
    {
        public EntityKey(IEntity entity)
        {
            Type = entity.GetType();
            Id = entity.Id;
            Version = entity.Version;
        }

        public EntityKey()
        {
            
        }
        public Type Type { get; set; }
    }

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

    [Serializable]
    public class Relational : Model
    {
        //This is where all the entities are stored
        private readonly Dictionary<Type, Entities> _entitySets = new Dictionary<Type, Entities>();

        [Serializable]
        class Entities : SortedDictionary<Guid, IEntity> { }


        public void Create<T>() where T : IEntity
        {
            _entitySets.Add(typeof(T), new Entities());
        }

        /// <summary>
        /// Insert one or more entities
        /// </summary>
        /// <param name="entities"></param>
        public void Insert(params IEntity[] entities)
        {
            if (CanInsert(entities)) DoUpsert(entities);
        }

        /// <summary>
        /// Update one or more existing entities by replacing if id, type and version match exactly.
        /// </summary>
        /// <param name="entities"></param>
        public void Update(params IEntity[] entities)
        {
            if (CanUpdate(entities)) DoUpsert(entities);
        }

        /// <summary>
        /// insert or update one or more entities as a single ACID transaction
        /// </summary>
        /// <param name="entities"></param>
        public void Upsert(params IEntity[] entities)
        {
            if (CanUpsert(entities)) DoUpsert(entities);
        }

        /// <summary>
        /// Delete one or more entities as a single ACID transaction
        /// </summary>
        /// <param name="entities"></param>
        public void Delete(params IEntity[] entities)
        {
            if (CanDelete(entities)) DoDelete(entities);
        }

        /// <summary>
        /// Execute multiple inserts, updates, deletes as a single ACID transaction
        /// </summary>
        /// <param name="batch"></param>
        public void Execute(Batch batch)
        {
            if (!CanExecute(batch)) throw new CommandAbortedException("Invalid batch");
            DoUpsert(batch.Updates);
            DoUpsert(batch.Inserts);
            DoDelete(batch.Deletes);
        }

        /// <summary>
        /// true as long as none of the ids exists
        /// </summary>
        private bool CanInsert(IEnumerable<IEntity> entities)
        {
            return entities.All(CanInsert);
        }

        private bool CanInsert(IEntity entity)
        {
            var set = ByType(entity.GetType()); 
            return ! set.ContainsKey(entity.Id);
        }

        private bool CanUpsert(IEntity[] entities)
        {
            return entities.All(CanUpsert);
        }

        private bool CanUpsert(IEntity entity)
        {
            return CanInsert(entity) || CanUpdate(entity);
        }


        private bool CanUpdate(IEnumerable<IEntity> entities)
        {
            return entities.All(CanUpdate);
        }

        private bool CanUpdate(IEntity entity)
        {
            var set = ByType(entity.GetType());
            IEntity target;
            return set.TryGetValue(entity.Id, out target) && target.Version == entity.Version;
        }

        private void DoUpsert(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = ByType(entity.GetType());
                set[entity.Id] = entity;
                entity.Version++;
            }
        }

        /// <summary>
        /// True if every entity exists and has a matching version
        /// </summary>
        private bool CanDelete(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = ByType(entity.GetType());
                if (!set.ContainsKey(entity.Id) || set[entity.Id].Version != entity.Version) return false;
            }
            return true;
        }

        private void DoDelete(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = ByType(entity.GetType());
                set.Remove(entity.Id);
            }
        }

        private bool CanExecute(Batch batch)
        {
            return CanDelete(batch.Deletes) &&
            CanInsert(batch.Inserts) &&
            CanUpdate(batch.Updates);
        }

        private Entities ByType(Type entityType)
        {
            Entities result;
            _entitySets.TryGetValue(entityType, out result);
            if (result == null) throw new CommandAbortedException("No such entity type: " + entityType.FullName);
            return result;
        }
    }
}
