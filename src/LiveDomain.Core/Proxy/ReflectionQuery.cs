using System;

namespace LiveDomain.Core.Proxy
{
	[Serializable]
	public class ReflectionQuery<M> : Query<M,object> where M : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ReflectionQuery(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		#region Overrides of Query<Model,object>

		protected override object Execute(M m)
		{
			var method = ReflectionHelper.ResolveMethod(m.GetType(), MethodName);
			return method.Invoke(m, Arguments);
		}

		#endregion
	}
}