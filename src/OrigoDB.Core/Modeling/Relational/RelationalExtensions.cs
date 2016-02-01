namespace OrigoDB.Core.Modeling.Relational
{
    public static class RelationalExtensions
    {
        /// <summary>
        /// Execute a batch in an all or nothing ACID transaction. Version field of each entity is incremented after a successful
        /// Exceptions: OptimisticConcurrencyException if there are version conflicts or CommandAbortedException if any entity type is undefined
        /// </summary>
        /// <param name="db"></param>
        /// <param name="batch"></param>
        public static void Execute(this RelationalModel db, Batch batch)
        {
            db.DoExecute(batch);
            batch.Inserts.ForEach(i => i.Version++);
            batch.Updates.ForEach(u => u.Version++);
        }

        public static void Update(this RelationalModel db, params IEntity[] entities)
        {
            db._Update(entities);
            foreach (var entity in entities) entity.Version++;
        }

        public static void Insert(this RelationalModel db, params IEntity[] entities)
        {
            db._Insert(entities);
            foreach (var entity in entities) entity.Version++;
        }
    }
}