using System;

namespace OrigoDB.Core.Models
{
    [Serializable]
    public class BinaryMessage : Message, IImmutable
    {
        public readonly byte[] Bytes;

        public BinaryMessage(byte[] bytes, String subject = "") : base(subject)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            Bytes = bytes;
        }
    }
}