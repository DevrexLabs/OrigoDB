using System;

namespace OrigoDB.Core.Modeling.Relational
{
    /// <summary>
    /// Identifies an entity by key, version and type. Used by Delete to avoid passing data fields when id is sufficient.
    /// </summary>
    [Serializable]
    public class EntityKey : Entity
    {

        public Type Type { get; set; }


        public EntityKey(IEntity entity)
        {
            Type = entity.GetType();
            Id = entity.Id;
            Version = entity.Version;
        }

        public EntityKey(){}

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityKey)) return false;
            EntityKey e = (EntityKey) obj;
            return e.Id == Id && e.Type == Type && e.Version == Version;

        }
    }
}