using System;

namespace OrigoDB.Core.Modeling.Relational
{
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
}