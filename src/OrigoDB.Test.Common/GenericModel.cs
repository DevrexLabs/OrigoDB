using System;
using System.Collections.Generic;
using OrigoDB.Core;

namespace OrigoDB.Test.Common
{
    [Serializable]
    public class GenericModel<TEntity> : Model where TEntity: Entity
    {
        internal IList<TEntity> Entities { get; set; }

        public GenericModel()
        {
            Entities = new List<TEntity>();
        }
    }
}