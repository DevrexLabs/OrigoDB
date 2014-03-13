using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OrigoDB.Core.Linq
{
	public static class CompiledLinqExtensions
	{
		//One compiler per model type, normally just one per process
		static Dictionary<Type, CachingLinqCompiler> _compilers = new Dictionary<Type, CachingLinqCompiler>();

		private static CachingLinqCompiler GetCompilerFor(Type type)
		{
			lock (_compilers)
			{
				if (!_compilers.ContainsKey(type)) _compilers.Add(type, new CachingLinqCompiler(type));
				return _compilers[type];
			}
		}

		internal static R Execute<R>(this Engine engine, Type type, string query, params object[] args)
		{
			return (R)Execute(engine, type, query, args);
		}

		internal static object Execute(this Engine engine, Type type, string query, params object[] args)
		{
			MethodInfo methodInfo = GetCompilerFor(type).GetCompiledQuery(query, args);

			//insert the  engine as first argument
			object[] methodArgs = new object[] { engine, args };
			return methodInfo.Invoke(null, methodArgs);
		}

		public static TResult Execute<TModel, TResult>(this Engine<TModel> engine, string query, params object[] args) where TModel : Model
		{
			return (TResult)Execute(engine, typeof(TModel), query, args);
		}

		public static object Execute<TModel>(this Engine<TModel> engine, string query, params object[] args) where TModel : Model
		{
			return Execute(engine, typeof(TModel), query, args);
		}
	}
}
