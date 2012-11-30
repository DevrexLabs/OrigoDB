using System;

namespace LiveDomain.Core
{
    public sealed class OptimizedKernel : Kernel
    {

        public OptimizedKernel(EngineConfiguration config, IStore store)
            : base(config, store)
        {
            
        }

        public override object ExecuteCommand(Command command)
        {
            try
            {
                _commandJournal.Append(command);
                _synchronizer.EnterUpgrade();
                command.PrepareStub(_model);
                _synchronizer.EnterWrite();
                object result = command.ExecuteStub(_model);
                return result;

            }
            finally
            {
                _synchronizer.Exit();
            }
        }
    }
}