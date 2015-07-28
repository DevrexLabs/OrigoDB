namespace OrigoDB.Core
{
    /// <summary>
    /// Interface for synchronizing reads and writes consumed by the Engine and Kernel
    /// </summary>
    public interface ISynchronizer
    {
        void EnterRead();
        void EnterUpgrade();
        void EnterWrite();
        void Exit();
    }
}
