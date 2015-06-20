using System;

namespace OrigoDB.Core
{

    public interface IImmutabilityCommandWithResult
    {
        /// <summary>
        /// Execute a command that returns results
        /// </summary>
        /// <param name="model">The initial state</param>
        /// <returns>a new model reflecting the modified state and a result</returns>
        Tuple<Model,object> Execute(Model model);
    }
}