using System;

namespace OrigoDB.Core.Types.Messaging
{
    [Serializable]
    public class DequeueCommand : Command<MessageBroker, Message>
    {
        public readonly string QueueName;

        public DequeueCommand(string queue)
        {
            QueueName = queue;
        }

        public override Message Execute(MessageBroker model)
        {
            return model.Dequeue(QueueName);
        }
    }
}
