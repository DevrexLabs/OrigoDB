using System;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core.Modeling.Relational
{
    [Serializable]
    public class RelationalModel : Model
    {
        //This is where all the entities are stored
        private readonly Dictionary<Type, EntitySet> _entitySets = new Dictionary<Type, EntitySet>();

        [Serializable]
        class EntitySet : SortedDictionary<Guid, IEntity> { }


        [NoProxy]
        public IQueryable<T> From<T>() where T : IEntity
        {
            return Of(typeof(T)).Values.Cast<T>().AsQueryable();
        }

        /// <summary>
        /// Lookup an entity by Type and Id, the type must exist or an exception will be thrown
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns>true if the entity </returns>
        public T TryGetById<T>(Guid id) where T : IEntity
        {
            IEntity untyped;
            Of(typeof(T)).TryGetValue(id, out untyped);
            return  (T) untyped;
        }

        /// <summary>
        /// True if the EntitySet with type T exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Exists<T>() where T : IEntity
        {
            return _entitySets.ContainsKey(typeof (T));
        }

        /// <summary>
        /// Create EntitySet unless it already exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>true if entityset was created</returns>
        [Command]
        public bool Create<T>() where T : IEntity
        {
            bool shouldCreate = ! Exists<T>();
            if (shouldCreate) _entitySets.Add(typeof(T), new EntitySet());
            return shouldCreate;
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
            var set = Of(entity.GetType()); 
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
            var set = Of(entity.GetType());
            IEntity target;
            return set.TryGetValue(entity.Id, out target) && target.Version == entity.Version;
        }

        private void DoUpsert(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = Of(entity.GetType());
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
                var set = Of(entity.GetType());
                if (!set.ContainsKey(entity.Id) || set[entity.Id].Version != entity.Version) return false;
            }
            return true;
        }

        private void DoDelete(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = Of(entity.GetType());
                set.Remove(entity.Id);
            }
        }

        private bool CanExecute(Batch batch)
        {
            return CanDelete(batch.Deletes) &&
            CanInsert(batch.Inserts) &&
            CanUpdate(batch.Updates);
        }

        private EntitySet Of(Type type, EntitySet @default = null)
        {
            EntitySet result;
            _entitySets.TryGetValue(type, out result);
            result = result ?? @default;
            if (result == null) throw new CommandAbortedException("No such entity type: " + type.FullName);
            return result;
        }
    }
}
