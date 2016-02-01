using System;

namespace OrigoDB.Core.Modeling.Relational
{
    [Serializable]
    public class MissingTypesException : CommandAbortedException
    {
        public readonly Type[] Types;

        public MissingTypesException(Type[] missingTypes)
        {
            Types = missingTypes;
        }
    }
}
