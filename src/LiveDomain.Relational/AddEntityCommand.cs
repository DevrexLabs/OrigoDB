using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace LiveDomain.Relational
{
    [Serializable]
    public class RelationalModel : Model
    {

        Dictionary<Type, EntitySet> _entitySets;

        public EntitySet<T> SetOf<T>()
        {
            return (EntitySet<T>)_entitySets[typeof(T)];
        }

        //public EntitySet<Category> Categories{ get; private set; }

        public void AddSetOf<T>()
        {
            //TODO: Throw if exists
            _entitySets.Add(typeof(T), new EntitySet<T>());
        }

        public RelationalModel()
        {
            _entitySets = new Dictionary<Type, EntitySet>();

            //Categories = new EntitySet<Category>();
            //_entitySets.Add(typeof(Category), Categories);
        }

        protected void InitTypeMap()
        {
            //TODO: Use reflection to find all properties of type EntitySet<T>
            //      and insert into the typeMap
        }
    }

    [Serializable]
    public class MyRelationalModel : RelationalModel
    {
        public MyRelationalModel()
        {
            AddSetOf<Category>();
            AddSetOf<User>();
            AddSetOf<Task>();
        }
    }

    [Serializable]
    public class AddEntityCommand<T> : CommandWithResult<RelationalModel, T>   
    {
        public readonly T Entity;

        protected override T Execute(RelationalModel db)
        {
            var entitySet = db.SetOf<T>();

            entitySet.Add(Entity);
            return Entity;
        }

        public AddEntityCommand(T entity)
        {
            Entity = entity;
        }
    }
}
