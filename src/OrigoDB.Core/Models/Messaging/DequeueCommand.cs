using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Models
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
