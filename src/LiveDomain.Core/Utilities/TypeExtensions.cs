using System;
using System.Collections.Generic;

namespace LiveDomain.Core.Utilities
{
    internal static class TypeExtensions
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
                                                               typeof(bool)
                                                           };

        public static bool InheritsOrImplements(this Type type, Type t)
        {
            //http://stackoverflow.com/posts/4897426/revisions
            throw new NotImplementedException();
        }


        public static bool IsImmutable(this Type type)
        {
            return _immutableTypes.Contains(type);
        }
    }
}