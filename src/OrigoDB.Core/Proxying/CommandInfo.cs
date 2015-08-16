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

        protected override object Execute(IEngine<T> engine, string signature, object command, IMethodCallMessage methodCallMessage)
        {
            if (command == null)
            {
               var proxyCommand =  new ProxyCommand<T>(signature, methodCallMessage.Args, methodCallMessage.MethodBase.GetGenericArguments());
               proxyCommand.ResultIsIsolated = ResultIsIsolated;
               command = proxyCommand;
            }
            return engine.Execute((Command)command);
        }
    }
}