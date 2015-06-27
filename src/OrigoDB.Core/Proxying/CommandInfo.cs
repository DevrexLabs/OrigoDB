using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace OrigoDB.Core.Proxying
{
    internal sealed class CommandInfo<T> : OperationInfo<T> where T:Model
    {
        public CommandInfo(MethodInfo methodInfo, OperationAttribute attribute)
            : base(methodInfo, attribute)
        {

        }

        protected override object Execute(IEngine<T> engine, string signature, object operation, IMethodCallMessage methodCallMessage)
        {
            var command = (Command) operation;
            command = command ?? new ProxyCommand<T>(signature, methodCallMessage.Args, methodCallMessage.MethodBase.GetGenericArguments());
            command.ResultIsSafe = !OperationAttribute.CloneResult;
            return engine.Execute(command);
        }
    }
}