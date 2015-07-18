using System;
using System.Linq;
using System.Timers;

namespace OrigoDB.Core.Models
{
    public static class MessageBrokerExtensions
    {
        public static Timer CreateQueueReader(this MessageBroker queues, string queueName, Action<Message> onMessage)
        {
            var timer = new Timer(500);
            timer.Elapsed += (sender, args) =>
            {
                var message = queues.Dequeue(queueName);
                if (message != null) onMessage.Invoke(message);
            };
            timer.Start();
            return timer;
        }

        public static Timer CreateTopicReader(this MessageBroker queues, string topicName, Guid subsscription,
            Action<Message[]> onMessages)
        {
            var timer = new Timer(500);
            timer.Elapsed += (sender, args) =>
            {
                var messages = queues.Poll(subsscription, topicName);
                if (messages.Any()) onMessages.Invoke(messages);
            };
            timer.Start();
            return timer;
        }
    }
}