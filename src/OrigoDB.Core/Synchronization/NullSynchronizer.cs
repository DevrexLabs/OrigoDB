namespace OrigoDB.Core
{
    /// <summary>
    /// No synchronization at all. Used with immutable models or when synchronization
    /// is managed at the client level.
    /// </summary>
    public class NullSynchronizer : ISynchronizer
    {

        public void EnterRead()
        {
            
        }

        public void EnterUpgrade()
        {
            
        }

        public void EnterWrite()
        {
            
        }

        public void Exit() { }

    }
}
