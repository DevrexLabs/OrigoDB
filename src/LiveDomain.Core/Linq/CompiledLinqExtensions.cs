using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LiveDomain.Core.Linq
{
    public static class CompiledLinqExtensions
    {
        //One compiler per model type, normally just one per process
        static Dictionary<Type, object> _compilers = new Dictionary<Type, object>();

        private static CachingLinqCompiler<M> GetCompilerFor<M>(M model = null) where M : Model
        {
            Type type = typeof (M);
            if(!_compilers.ContainsKey(type)) _compilers.Add(type, new CachingLinqCompiler<M>());
            return (CachingLinqCompiler<M>)_compilers[type];

        }

        public static R Execute<M,R>(this Engine<M> engine, string query, params object[] args ) where M : Model
        {
            return (R) Execute<M>(engine, query,args);
        }

        public static object Execute<M>(this Engine<M> engine, string query, params object[] args) where M : Model
        {
        try
        {
            MethodInfo methodInfo = GetCompilerFor<M>().GetCompiledQuery(query, args);

            //insert the  engine as first argument
            object[] methodArgs = new object[]{engine, args};
            return methodInfo.Invoke(null, methodArgs);
        }
        catch (Exception)
        {

            throw;
        }
    }
    }
}
