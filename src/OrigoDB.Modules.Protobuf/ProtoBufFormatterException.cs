using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.Serialization;

namespace OrigoDB.Modules.ProtoBuf
{
    [Serializable]
    public sealed class ProtoBufFormatterException : Exception
    {
        public ProtoBufFormatterException()
            : base()
        {
        }

        public ProtoBufFormatterException(string message)
            : base(message)
        {
        }

        public ProtoBufFormatterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [SecuritySafeCritical]
        private ProtoBufFormatterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {        
        }

    }
}
