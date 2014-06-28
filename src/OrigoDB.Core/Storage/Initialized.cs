namespace OrigoDB.Core.Storage
{
    public abstract class Initialized
    {
        protected bool IsInitialized = false;


        public virtual void Initialize()
        {
            IsInitialized = true;
        }

        public void EnsureInitialized()
        {
            if (!IsInitialized) Initialize();
        }
    }
}