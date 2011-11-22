using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{


    public enum CompressionMethod
    {
        /// <summary>
        /// Default, no compression
        /// </summary>
        None,

        /// <summary>
        /// Use .NET Gzip compression
        /// <remarks>Unsupported at the moment</remarks>
        /// </summary>
        GZip

        //TODO: Add 7zip support
    }
}
