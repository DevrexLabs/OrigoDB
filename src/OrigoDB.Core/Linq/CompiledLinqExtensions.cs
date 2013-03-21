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

		public static R Execute<M, R>(this Engine<M> engine, string query, params object[] args) where M : Model
		{
			return (R)Execute(engine, typeof(M), query, args);
		}

		public static object Execute<M>(this Engine<M> engine, string query, params object[] args) where M : Model
		{
			return Execute(engine, typeof(M), query, args);
		}
	}
}
