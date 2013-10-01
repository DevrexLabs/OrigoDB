using System;

namespace OrigoDB.Core
{
   
    /// <summary>
    /// Base class for commands supporting immutable model mode
    /// </summary>
    /// <typeparam name="M"></typeparam>
    [Serializable]
    public abstract class ImmutabilityCommand<M>: Command<M> where M : Model
    {

        public abstract M ExecuteImmutably(M model);

        protected internal override void Execute(M model)
        {
            throw new NotImplementedException("Can only be executed by ImmutabilityKernel");
        }
    }

    [Serializable] 
    public abstract class ImmutabilityCommand<M, R> : Command<M, R> where M : Model
    {
        public abstract Tuple<M, R> ExecuteImmutably(M model);

        protected internal override R Execute(M model)
        {
            throw new InvalidOperationException("Can only be executed by ImmutabilityKernel");
        }
    }
}