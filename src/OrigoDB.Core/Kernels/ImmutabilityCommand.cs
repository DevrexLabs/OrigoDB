using System;

namespace OrigoDB.Core
{

    internal interface IImmutabilityCommand
    {
        Model Execute(Model model);
    }

    internal interface IImmutabilityCommandWithResult
    {
        Tuple<Model,object> Execute(Model model);
    }
   
    /// <summary>
    /// Base class for commands supporting immutable model mode
    /// </summary>
    /// <typeparam name="M"></typeparam>
    [Serializable]
    public abstract class ImmutabilityCommand<M> : Command<M>, IImmutabilityCommand where M : Model
    {

        public abstract void Execute(M model, out M result);

        protected internal override void Execute(M model)
        {
            throw new NotImplementedException("Can only be executed by ImmutabilityKernel");
        }

        internal override void Redo(ref Model model)
        {
            PrepareStub(model);
            model = ((IImmutabilityCommand) this).Execute(model);
        }

        Model IImmutabilityCommand.Execute(Model model)
        {
            M result;
            Execute((M) model, out result);
            return result;
        }
    }

    [Serializable] 
    public abstract class ImmutabilityCommand<M, R> : Command<M, R>, IImmutabilityCommandWithResult where M : Model
    {
        public abstract R Execute(M model, out M newModel);

        protected internal override R Execute(M model)
        {
            throw new InvalidOperationException("Can only be executed by ImmutabilityKernel");
        }

        internal override void Redo(ref Model model)
        {
            PrepareStub(model);
            model = ((IImmutabilityCommandWithResult) this).Execute(model).Item1;
        }

        Tuple<Model,object> IImmutabilityCommandWithResult.Execute(Model model)
        {
            M resultingModel;
            object result = Execute((M) model, out resultingModel);
            return Tuple.Create((Model)resultingModel, result);
        }
    }
}