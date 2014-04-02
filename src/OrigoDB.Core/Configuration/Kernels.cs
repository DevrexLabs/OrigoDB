namespace OrigoDB.Core.Configuration
{
    /// <summary>
    /// Configuration property deciding which type of <see cref="OrigoDB.Core.Kernel"/> to use.
    /// </summary>
    public enum Kernels
    {
        /// <summary>
        /// Execute commands after writing to the journal, see <see cref="OrigoDB.Core.OptimisticKernel"/>
        /// </summary>
        Optimistic,

        /// <summary>
        /// Test commands on a copy of the model before applying to the real model, see <see cref="OrigoDB.Core.RoyalFoodTaster"/>
        /// </summary>
        RoyalFoodTaster,

        Immutability
    }
}
