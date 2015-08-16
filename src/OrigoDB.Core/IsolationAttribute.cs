using System;

namespace OrigoDB.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IsolationAttribute : Attribute
    {
        public Isolation Isolation{ get; set; }

        public IsolationAttribute(Isolation isolation)
        {
            Isolation = isolation;
        }
    }
}