using System;

namespace LiveDomain.Core
{
    public sealed class PessimisticKernel : Kernel
    {
        public PessimisticKernel(EngineConfiguration config, IStore store)
            : base(config, store)
        {
            
        }

        public override object ExecuteCommand(Command command)
        {
            try
            {
                _synchronizer.EnterUpgrade();
                command.PrepareStub(_model);
                _synchronizer.EnterWrite();
                object result = command.ExecuteStub(_model);
                EnsureSafeResults(ref result, command as IOperationWithResult);
                _commandJournal.Append(command);
                return result;
            }
            catch (TimeoutException)
            {
                //ThrowIfDisposed();
                throw;
            }
            catch (CommandAbortedException) { throw; }
            catch (Exception ex)
            {
                Restore(() => (Model)Activator.CreateInstance(_model.GetType())); //TODO: Or shutdown based on setting
                throw new CommandAbortedException("Command threw an exception, state was rolled back, see inner exception for details", ex);
            }
            finally
            {
                _synchronizer.Exit();
            }

        }
    }
}