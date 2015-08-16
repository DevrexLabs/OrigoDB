using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// Marks a type as immutable. Immuable types won't be cloned when Isolation is set to CloneStrategy.Auto
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class ImmutableAttribute : Attribute { }
}
