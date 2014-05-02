using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{

    /// <summary>
    /// A simple IOC registry used by the EngineConfiguration to handle modules
    /// </summary>
    public class TeenyIoc
    {
        /// <summary>
        /// Arguments can be passed during resolution
        /// </summary>
        public class Args : Dictionary<string, object> { }

        private Dictionary<Type, Dictionary<string, Func<Args, object>>> _registry
            = new Dictionary<Type, Dictionary<string, Func<Args, object>>>();


        /// <summary>
        /// Register a factory, pass a name to create a named registration
        /// </summary>
        public void Register<T>(Func<Args, T> factory, string name = "") where T : class
        {
            Type t = typeof(T);
            if (!_registry.ContainsKey(t)) _registry[t] = new Dictionary<string, Func<Args, object>>();
            _registry[t][name] = factory;
        }


        /// <summary>
        /// Invokes the factory function registered for type T and a specific name
        /// </summary>
        public T Resolve<T>(string name) 
        {
            return Resolve<T>(null, name);
        }


        public bool CanResolve<T>(string name = "") where T : class
        {
            var t = typeof (T);
            return _registry.ContainsKey(t) && _registry[t].ContainsKey(name);
        }

        /// <summary>
        /// Invoke factory function registered for type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">optional args to the factory function</param>
        /// <param name="name">optional named registration</param>
        /// <returns>the result of invoking the factory</returns>
        public T Resolve<T>(Args args = null, string name = "")
        {
            args = args ?? new Args();
            Type t = typeof(T);
            if (!_registry.ContainsKey(t)) throw new InvalidOperationException();

            if (!_registry[t].ContainsKey(name)) throw new InvalidOperationException();

            var factory = _registry[t][name];
            return (T)factory.Invoke(args);
        }
    }
}