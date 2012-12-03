using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Configuration
{
    /// <summary>
    /// Configuration property deciding which type of <see cref="LiveDomain.Core.Kernel"/> to use.
    /// <seealso cref="LiveDomain.Core.OptimisticKernel"/> or <seealso cref="LiveDomain.Core.PessimisticKernel"/>
    /// </summary>
    public enum KernelMode
    {
        Pessimistic,
        Optimistic
    }
}
