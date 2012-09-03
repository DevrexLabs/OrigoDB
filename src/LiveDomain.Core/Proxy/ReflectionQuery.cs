using System;

namespace LiveDomain.Core.Proxy
{
	[Serializable]
	public class ReflectionQuery<T> : Query<T,object> where T : Model
	{
		public string MethodName { get; set; }
		public object[] Arguments { get; set; }

		public ReflectionQuery(string methodName, object[] inArgs)
		{
			MethodName = methodName;
			Arguments = inArgs;
		}

		#region Overrides of Query<Model,object>

		protected override object Execute(T m)
		{
			var method = ReflectionHelper.ResolveMethod(m.GetType(), MethodName);
			return method.Invoke(m, Arguments);
		}

		#endregion
	}
}