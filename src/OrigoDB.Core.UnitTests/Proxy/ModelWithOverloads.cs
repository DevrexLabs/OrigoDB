using System;

namespace OrigoDB.Core.Test
{
    [Serializable]
    public class ModelWithOverloads : Model
    {
        public void Meth()
        {
            
        }

        public void Meth(object state)
        {
            
        }

        public void Meth(params object[] stuff)
        {
            
        }

        public void Meth(ref object state)
        {
            
        }

    }
}