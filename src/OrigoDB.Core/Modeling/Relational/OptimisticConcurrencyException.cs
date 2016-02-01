using System;

namespace OrigoDB.Core.Modeling.Relational
{
    [Serializable]
    public class OptimisticConcurrencyException : CommandAbortedException
    {
        public readonly Conflicts Conflicts;
        public OptimisticConcurrencyException(Conflicts conflicts)
        {
            Conflicts = conflicts;
        }
    }
}