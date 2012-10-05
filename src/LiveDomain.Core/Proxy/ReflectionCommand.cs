using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace LiveDomain.Core.Proxy
{
	[Serializable]
    public class ReflectionCommand<M> : CommandWithResult<M, object> where M : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ReflectionCommand(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		#region Overrides of CommandWithResult<T,object>

		protected internal override object Execute(M model)
		{
			var method = ReflectionHelper.ResolveMethod(model.GetType(), MethodName);
			return method.Invoke(model, Arguments);
		}

		#endregion
	}
}
