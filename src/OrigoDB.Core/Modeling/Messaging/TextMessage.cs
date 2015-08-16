using System;

namespace OrigoDB.Core.Modeling.Messaging
{
    [Serializable, Immutable]
    public class TextMessage : Message
    {
        public readonly string Body;

        public TextMessage(String body, String subject = "") : base(subject)
        {
            Body = body;
        }
    }
}