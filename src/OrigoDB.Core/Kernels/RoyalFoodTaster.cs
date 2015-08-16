using System;
using System.Runtime.Serialization;

namespace OrigoDB.Core
{
    /// <summary>
    /// Keeps an identical copy of the model to try the commands.
    /// </summary>
    public sealed class RoyalFoodTaster : OptimisticKernel
    {

        /// <summary>
        /// An identical copy of the model
        /// </summary>
        Model _foodTaster;

        readonly IFormatter _formatter;

        public RoyalFoodTaster(EngineConfiguration config, Model model)
            : base(config, model)
        {
            _formatter = config.CreateFormatter(FormatterUsage.Snapshot);
            _foodTaster = _formatter.Clone(Model);
        }

        /// <summary>
        /// Apply the command to the food taster. If it succeeds, apply to the real model.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override object ExecuteCommand(Command command)
        {

            try
            {
                command.PrepareStub(_foodTaster);
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
                _foodTaster = _formatter.Clone(Model); //reset
                throw new CommandAbortedException("Royal taster died of food poisoning, see inner exception for details", ex);
            }

            return base.ExecuteCommand(command);
        }
    }
}