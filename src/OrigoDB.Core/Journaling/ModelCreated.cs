using System;

namespace OrigoDB.Core.Journaling
{


    /// <summary>
    /// Used to initiate the model during restore when no snapshot is present.
    /// </summary>
    [Serializable]
    public class ModelCreated
    {
        public readonly Type Type;

        public ModelCreated(Type type)
        {
            Type = type;
        }
    }
}