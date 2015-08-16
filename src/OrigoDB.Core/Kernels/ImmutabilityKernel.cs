using System;

namespace OrigoDB.Core
{
    public sealed class ImmutabilityKernel : OptimisticKernel
    {
        public ImmutabilityKernel(EngineConfiguration config, Model model) :
            base(config, model)
        {
        }

        public override object ExecuteCommand(Command command)
        {
            object result = null;
            try
            {
                Model modelOut = null;
                if (command is IImmutabilityCommand)
                {
                    var typedCommand = command as IImmutabilityCommand;
                    modelOut = typedCommand.Execute(Model);
                }
                else if (command is IImmutabilityCommandWithResult)
                {
                    var typedCommand = command as IImmutabilityCommandWithResult;
                    var tuple = typedCommand.Execute(Model);
                    UnpackTuple(tuple, out modelOut, out result);
                }
                else throw new InvalidOperationException("Command type not supported by this kernel");
                if (modelOut == null) throw new InvalidOperationException("Command returned null model");
                modelOut.Revision = Model.Revision + 1;
                Model = modelOut;
                return result;
            }
            catch (Exception ex)
            {
                throw new CommandAbortedException("Command failed, see inner exception for details", ex);
            }
        }


        protected override void EnsureIsolation(ref object result, object operation)
        {
            //noop, result is immutable and so is model, no cloning necessary
            //Strategies 
        }

        private void UnpackTuple(object tuple, out Model model, out object result)
        {
            var type = tuple.GetType();
            model = (Model)type.GetProperty("Item1").GetGetMethod().Invoke(tuple, null);
            result = type.GetProperty("Item2").GetGetMethod().Invoke(tuple, null);
        }

    }
}