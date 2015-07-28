using System;

namespace OrigoDB.Core.Types.Messaging
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