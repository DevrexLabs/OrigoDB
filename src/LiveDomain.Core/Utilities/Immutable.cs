using System;
using System.Collections.Generic;

namespace LiveDomain.Core.Utilities
{
    public static class Immutable
    {

        private static HashSet<Type> _immutableTypes = new HashSet<Type>()
                                                           {
                                                               typeof (Int32),
                                                               typeof(Int64),
                                                               typeof(Int16),
                                                               typeof(UInt32),
                                                               typeof(UInt64),
                                                               typeof(UInt16),
                                                               typeof(byte),
                                                               typeof(DateTime),
                                                               typeof(TimeSpan),
                                                               typeof(string),
                                                               typeof(bool),
                                                               typeof(Guid),
                                                               typeof(object)
                                                           };


        public static void Register<T>()
        {
            lock (_immutableTypes)
            {
                _immutableTypes.Add(typeof (T));
            }
        }

        public static bool IsRegistered<T>()
        {
            lock (_immutableTypes)
            {
                return _immutableTypes.Contains(typeof(T)); 
            }
        }

        public static bool UnRegister<T>()
        {

            lock (_immutableTypes)
            {
                return _immutableTypes.Remove(typeof(T)); 
            }
        }

        public static bool IsImmutable(this object @object)
        {
            var type = @object.GetType();
            return @object is IImmutable  || type.IsEnum || type.IsPrimitive || _immutableTypes.Contains(type);
        }
    }
}