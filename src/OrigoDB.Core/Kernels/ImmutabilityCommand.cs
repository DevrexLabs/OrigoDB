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
    /// <typeparam name="TModel"></typeparam>
    [Serializable]
    public abstract class ImmutabilityCommand<TModel> : Command<TModel>, IImmutabilityCommand where TModel : Model
    {

        public abstract void Execute(TModel model, out TModel result);

        public override void Execute(TModel model)
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
            TModel result;
            Execute((TModel) model, out result);
            return result;
        }
    }

    [Serializable] 
    public abstract class ImmutabilityCommand<TModel, TResult> : Command<TModel, TResult>, IImmutabilityCommandWithResult where TModel : Model
    {
        public abstract TResult Execute(TModel model, out TModel newModel);

        public override TResult Execute(TModel model)
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
            TModel resultingModel;
            object result = Execute((TModel) model, out resultingModel);
            return Tuple.Create((Model)resultingModel, result);
        }
    }
}