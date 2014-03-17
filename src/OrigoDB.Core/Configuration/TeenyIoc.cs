using System;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    public class TeenyIoc
    {
        public class Args : Dictionary<string, object> { }
        private Dictionary<Type, Dictionary<string, Func<Args, object>>> _registry
            = new Dictionary<Type, Dictionary<string, Func<Args, object>>>();

        public void Register<T>(Func<Args, T> factory, string name = "") where T : class
        {
            Type t = typeof(T);
            if (!_registry.ContainsKey(t)) _registry[t] = new Dictionary<string, Func<Args, object>>();
            _registry[t][name] = factory;
        }


        public T Resolve<T>(string name)
        {
            return Resolve<T>(null, name);
        }

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