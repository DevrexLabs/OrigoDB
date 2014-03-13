using System;

namespace OrigoDB.Core
{


    /// <summary>
    /// A command modifies the state of the model.
    /// </summary>
    /// <typeparam name="TModel">The specific type of the model, derived from Model</typeparam>
    [Serializable]
    public abstract class Command<TModel> : Command
        where TModel : Model
    {

        protected internal virtual void Prepare(TModel model) { }

        internal override object ExecuteStub(Model model)
        {
            Execute(model as TModel);
            return null;
        }

        internal override void PrepareStub(Model model)
        {
            try
            {
                Prepare(model as TModel);
            }
            catch (Exception ex)
            {

                throw new CommandAbortedException("Exception thrown during Prepare(), see inner exception for details", ex);
            }
        }

        internal protected abstract void Execute(TModel model);
    }

}
