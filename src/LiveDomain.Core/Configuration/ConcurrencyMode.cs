using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    /// <summary>
    /// Engine chooses Locking strategy based on this setting
    /// </summary>
    public enum ConcurrencyMode
    {
        /// <summary>
        /// Allow access to one thread at a time for either reading or writing
        /// </summary>
        SingleReaderOrWriter,

        /// <summary>
        /// Allow multiple reader threads or 
        /// </summary>
        MultipleReadersOrSingleWriter,

        /// <summary>
        /// Allow any access, thread safety is controlled by client code
        /// </summary>
        MultipleReadersAndWriters
    }
}
