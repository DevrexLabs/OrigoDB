using System;

namespace OrigoDB.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IsolationAttribute : Attribute
    {
        public IsolationLevel Level{ get; set; }

        public IsolationAttribute(IsolationLevel isolation)
        {
            Level = isolation;
        }
    }
}