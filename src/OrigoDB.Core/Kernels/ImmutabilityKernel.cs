using System;

namespace OrigoDB.Core
{
    public class ImmutabilityKernel : OptimisticKernel
    {

        public ImmutabilityKernel(EngineConfiguration config, Model model) :
            base(config, model)
        {
        }

        public override object ExecuteCommand(Command command)
        {
            try
            {
                var args = new object[] {_model};
                object innerResult = command.GetType().GetMethod("ExecuteImmutably").Invoke(command, args);
                object result = null;

                if (innerResult.GetType() == _model.GetType())
                {
                    _model = (Model)innerResult;
                }
                else //assume result is a tuple
                {
                    Model candidateModel;
                    UnpackTuple(innerResult, out candidateModel, out result);
                    if (candidateModel.GetType() == _model.GetType()) _model = candidateModel;
                    else throw new Exception();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new CommandAbortedException("Command failed, see inner exception for details", ex);
            }
        }


        protected override void EnsureSafeResults(ref object result, IOperationWithResult operation)
        {
            //noop, result is immutable and so is model, no cloning necessary
        }

        private void UnpackTuple(object tuple, out Model model, out object result)
        {
            var type = tuple.GetType();
            model = (Model)type.GetProperty("Item1").GetGetMethod().Invoke(tuple, null);
            result = type.GetProperty("Item2").GetGetMethod().Invoke(tuple, null);
        }

    }
}