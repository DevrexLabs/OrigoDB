using System;

namespace OrigoDB.Core.Modeling.Relational
{
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
}