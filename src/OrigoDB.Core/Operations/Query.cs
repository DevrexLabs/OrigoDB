using System;

namespace OrigoDB.Core
{
    [Serializable]
    public abstract class Query : IOperationWithResult
    {
        public bool? ResultIsIsolated { get; set; }

        internal abstract object ExecuteStub(Model m);
    }
}