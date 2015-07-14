namespace OrigoDB.Core
{
    /// <summary>
    /// The foundation of immutable commands
    /// </summary>
    internal interface IImmutabilityCommand
    {
        /// <summary>
        /// An immutable model can only be replaced by a new model, never mutated
        /// </summary>
        /// <param name="model">The state before the command</param>
        /// <returns>The state after the command</returns>
        Model Execute(Model model);
    }
}