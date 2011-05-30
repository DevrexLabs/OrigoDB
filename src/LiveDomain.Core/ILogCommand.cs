using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    /// <summary>
    /// Enables commands to be prepared and executed without 
    /// generic type parameter M and ignoring return value
    /// </summary>
    internal interface ILogCommand
    {
        /// <summary>
        /// Prepare and execute the command
        /// </summary>
        /// <param name="model"></param>
        void Redo(Model model);
    }
}
