using System.Reflection;
using OrigoDB.Core;

namespace Proxying
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
            //todo: command.ResultIsSafe = !ProxyAttribute.CloneResult;
            return engine.Execute(command);
        }
    }
}