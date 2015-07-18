using System;

namespace OrigoDB.Core.Models
{
    [Serializable]
    public class TextMessage : Message, IImmutable
    {
        public readonly string Body;

        public TextMessage(String body, String subject = "") : base(subject)
        {
            Body = body;
        }
    }
}