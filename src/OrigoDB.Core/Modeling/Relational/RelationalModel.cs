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
            return For(typeof(T)).Values.Cast<T>().AsQueryable();
        }

        /// <summary>
        /// Lookup an entity by Type and Id, the type must exist or an exception will be thrown
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns>the entity or null if not found</returns>
        public T TryGetById<T>(Guid id) where T : IEntity
        {
            IEntity untyped;
            For(typeof(T)).TryGetValue(id, out untyped);
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
        internal void _Insert(params IEntity[] entities)
        {
            Conflicts conflicts = new Conflicts();
            if (!CanInsert(entities, conflicts)) 
                throw new OptimisticConcurrencyException(conflicts);
            DoUpsert(entities);
        }

        /// <summary>
        /// Update one or more existing entities by replacing if id, type and version match exactly.
        /// </summary>
        /// <param name="entities"></param>
        internal void _Update(params IEntity[] entities)
        {
            Conflicts conflicts = new Conflicts();
            if (!CanUpdate(entities, conflicts))
                throw new OptimisticConcurrencyException(conflicts);
            DoUpsert(entities);
        }


        /// <summary>
        /// Delete one or more entities as a single ACID transaction
        /// </summary>
        /// <param name="entities"></param>
        public void Delete(params IEntity[] entities)
        {
            Conflicts conflicts = new Conflicts();
            if (!CanDelete(entities, conflicts))
                throw new OptimisticConcurrencyException(conflicts);
            DoDelete(entities);
        }

        /// <summary>
        /// Execute multiple inserts, updates, deletes as a single ACID transaction
        /// </summary>
        /// <param name="batch"></param>
        public void DoExecute(Batch batch)
        {
            var missingTypes = batch.Types().Except(_entitySets.Keys).ToArray();
            if (missingTypes.Any()) throw new MissingTypesException(missingTypes);
            Conflicts conflicts = new Conflicts();
            if (!CanExecute(batch, conflicts))
                throw new OptimisticConcurrencyException(conflicts);
            DoUpsert(batch.Updates);
            DoUpsert(batch.Inserts);
            DoDelete(batch.Deletes);
        }

        /// <summary>
        /// Throw an exception if any of the keys exist
        /// </summary>
        private bool CanInsert(IEnumerable<IEntity> entities, Conflicts conflicts = null)
        {
            conflicts = conflicts ?? new Conflicts();
            int numConflicts = conflicts.Inserts.Count;
            foreach(var entity in entities)
            {
                var set = For(entity);
                IEntity existing;
                if (set.TryGetValue(entity.Id, out existing)) 
                    conflicts.Inserts.Add(new EntityKey(existing));
            }
            return numConflicts == conflicts.Inserts.Count;
        }

 
        private bool CanUpdate(IEnumerable<IEntity> entities, Conflicts conflicts = null)
        {
            conflicts = conflicts ?? new Conflicts();
            int numConflicts = conflicts.Updates.Count;
            foreach (var entity in entities)
            {
                var set = For(entity);
                if (!set.ContainsKey(entity.Id)) conflicts.Updates.Add(new EntityKey(entity){Version=0});
                else if (set[entity.Id].Version != entity.Version) conflicts.Updates.Add(new EntityKey(set[entity.Id]));
            }
            return numConflicts == conflicts.Updates.Count;

        }

        private void DoUpsert(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = For(entity);
                set[entity.Id] = entity;
                entity.Version++;
            }
        }

        /// <summary>
        /// True if every entity exists and has a matching version
        /// </summary>
        private bool CanDelete(IEnumerable<IEntity> entities, Conflicts conflicts = null)
        {
            conflicts = conflicts ?? new Conflicts();
            int numConflicts = conflicts.Deletes.Count;

            foreach (var entity in entities)
            {
                var set = For(entity);
                IEntity existing;
                if (!set.TryGetValue(entity.Id, out existing))
                {
                    conflicts.Deletes.Add(new EntityKey(entity) {Version = 0});
                }
                else if (existing.Version != entity.Version)
                {
                    conflicts.Deletes.Add(new EntityKey(existing));
                }
            }
            return numConflicts == conflicts.Deletes.Count;
        }

        private void DoDelete(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
            {
                var set = For(entity);
                set.Remove(entity.Id);
            }
        }

        private bool CanExecute(Batch batch, Conflicts conflicts)
        {
            return CanDelete(batch.Deletes, conflicts) &&
            CanInsert(batch.Inserts, conflicts) &&
            CanUpdate(batch.Updates, conflicts);
        }

        private EntitySet For(Type type, EntitySet @default = null)
        {
            EntitySet result;
            _entitySets.TryGetValue(type, out result);
            result = result ?? @default;
            if (result == null) throw new CommandAbortedException("No such entity type: " + type.FullName);
            return result;            
        }

        private EntitySet For(IEntity entity, EntitySet @default = null)
        {
            var type = entity is EntityKey
                ? ((EntityKey)entity).Type
                : entity.GetType();
            return For(type, @default);
        }
    }
}
