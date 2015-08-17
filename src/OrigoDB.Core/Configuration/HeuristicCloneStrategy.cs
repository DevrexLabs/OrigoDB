using System.Linq;

namespace OrigoDB.Core
{

    /// <summary>
    /// Clone unless we know isolation is guaranteed
    /// </summary>
    public sealed class HeuristicCloneStrategy : CloneStrategy
    {
        internal override void Apply(ref Command command)
        {
            if (!HasIsolationAttribute(command, IsolationLevel.Input) && ! command.GetType().HasImmutableAttribute())
                command = Formatter.Clone(command);
        }

        internal override void Apply(ref object result, object producer)
        {
            if (!result.GetType().IsIsolated() && !IsolatesResults(producer))
                result = Formatter.Clone(result);
        }

        private bool IsolatesResults(object producer)
        {
            return ByOperationWithResult(producer as IOperationWithResult) ||
                   HasIsolationAttribute(producer, IsolationLevel.Output);
        }

        private bool HasIsolationAttribute(object producer, IsolationLevel isolation)
        {
            var attribute = (IsolationAttribute)producer.GetType().GetCustomAttributes(typeof(IsolationAttribute), false).FirstOrDefault();
            return attribute != null && attribute.Level.HasFlag(isolation);
        }

        private bool ByOperationWithResult(IOperationWithResult operation)
        {
            return operation != null && 
                   operation.ResultIsIsolated.HasValue && 
                   operation.ResultIsIsolated.Value;
        }
    }
}