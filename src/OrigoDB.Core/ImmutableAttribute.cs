using System;

namespace OrigoDB.Core
{
    /// <summary>
    /// Immutable types don't need to be cloned to guarantee isolation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class ImmutableAttribute : Attribute { }
}
