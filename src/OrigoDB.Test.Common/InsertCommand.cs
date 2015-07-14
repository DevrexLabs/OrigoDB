using System;
using OrigoDB.Core;

namespace OrigoDB.Test.Common
{
    [Serializable]
    public class InsertCommand<TEntity> : Command<GenericModel<TEntity>> where TEntity: Entity
    {
        private TEntity Entity { get; set; }

        public InsertCommand(TEntity entity)
        {
            Entity = entity;
        }

        public override void Execute(GenericModel<TEntity> model)
        {
            try 
	        {	
                Entity.Id = new Guid();
		        model.Entities.Add(Entity);
	        }
	        catch (Exception ex)
	        {
	            Entity.Id = Guid.Empty;
		        throw;
	        }
        }
    }
}