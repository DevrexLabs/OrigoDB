using System;

namespace OrigoDB.Core
{
    public sealed class RoyalFoodTaster : OptimisticKernel
    {

        /// <summary>
        /// An identical copy of the model
        /// </summary>
        private Model _foodTaster;

        public override void Restore<M>(Func<M> constructor = null)
        {
            base.Restore(constructor);
            _foodTaster = _serializer.Clone(_model);
        }

        public RoyalFoodTaster(EngineConfiguration config, IStore store)
            : base(config, store)
        {

        }

        /// <summary>
        /// Apply the command to the food taster. If it succeeds, apply to the real model.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override object ExecuteCommand(Command command)
        {

            lock (_commandLock)
            {
                try
                {
                    command.ExecuteStub(_foodTaster); //outofmemory,commandaborted, unhandled user
                }
                catch (CommandAbortedException)
                {
                    throw;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _foodTaster = _serializer.Clone(_model);
                    throw new CommandFailedException("Foodtaster rejected command", ex);
                }

                return base.ExecuteCommand(command);
            }
        }
    }
}