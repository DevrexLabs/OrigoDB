using System.Reflection;

namespace OrigoDB.Core.Proxying
{
    internal sealed class CommandInfo<T> : OperationInfo<T> where T:Model
    {
        public CommandInfo(MethodInfo methodInfo, OperationAttribute attribute)
            : base(methodInfo, attribute)
        {

        }

        protected override object Execute(IEngine<T> engine, string signature, object operation, object[] args)
        {
            var command = (Command) operation;
            command = command ?? new ProxyCommand<T>(signature, args);
            command.ResultIsSafe = !OperationAttribute.CloneResult;
            return engine.Execute(command);
        }
    }
}