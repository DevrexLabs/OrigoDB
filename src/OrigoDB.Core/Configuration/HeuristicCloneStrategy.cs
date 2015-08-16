using System.Linq;

namespace OrigoDB.Core
{
    public sealed class HeuristicCloneStrategy : CloneStrategy {
        internal override void Apply(ref Command command)
        {
            if (HasIsolationAttribute(command, Isolation.Input))
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
                   HasIsolationAttribute(producer, Isolation.Output);
        }

        private bool HasIsolationAttribute(object producer, Isolation isolation)
        {
            var attribute = (IsolationAttribute)producer.GetType().GetCustomAttributes(typeof(IsolationAttribute), false).FirstOrDefault();
            return attribute != null && attribute.Isolation.HasFlag(isolation);
        }

        private bool ByOperationWithResult(IOperationWithResult operation)
        {
            return operation != null && 
                   operation.ResultIsIsolated.HasValue && 
                   operation.ResultIsIsolated.Value;
        }
    }
}