using System;

namespace OrigoDB.Core.Modeling.Messaging
{
    [Serializable, Immutable]
    public class BinaryMessage : Message
    {
        public readonly byte[] Bytes;

        public BinaryMessage(byte[] bytes, String subject = "") : base(subject)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            Bytes = bytes;
        }
    }
}