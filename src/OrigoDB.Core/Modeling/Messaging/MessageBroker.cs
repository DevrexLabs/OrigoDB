using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace OrigoDB.Core.Modeling.Messaging
{
    /// <summary>
    /// Message broker supporting any number of queues (competing consumers)
    /// or topics (multiple subscribers)
    /// </summary>
    [Serializable]
    public class MessageBroker : Model
    {
        [Serializable]
        class MessageQueue : Queue<Message> { }
        
        /// <summary>
        /// A topic is simply a set of subscribers, each with it's own message queue
        /// </summary>
        [Serializable]
        class Topic : Dictionary<Guid, MessageQueue> { }

        private readonly Dictionary<String, MessageQueue> _queues 
            = new Dictionary<string, MessageQueue>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<String, Topic> _topics
            = new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase);


        public void Subscribe(Guid subscriber, String topicName)
        {
            var topic = GetTopic(topicName);
            if (topic.ContainsKey(subscriber)) throw new CommandAbortedException("Already subscribed");
            topic[subscriber] = new MessageQueue();
        }

        /// <summary>
        /// remove subscriber from topic and return any remaining messages
        /// </summary>
        public Message[] Unsubscribe(Guid subscriber, String topicName)
        {
            var topic = GetTopic(topicName);
            MessageQueue q;

            if (!topic.TryGetValue(subscriber, out q))
            {
                throw new CommandAbortedException("No such subscriber");
            }
            topic.Remove(subscriber);
            return q.ToArray();
        }

        public void Enqueue(string queue, Message message)
        {
            var q = GetQueue(queue);
            q.Enqueue(message);
        }

        /// <summary>
        /// Get the next message in a given queue
        /// </summary>
        /// <param name="queue"></param>
        /// <returns>the next message or null if the queue is empty</returns>
        [Command(MapTo = typeof(DequeueCommand))]
        public Message Dequeue(string queue)
        {
            var q = GetQueue(queue);
            return q.Count > 0 ? q.Dequeue() : null;
        }

        /// <summary>
        /// Leave a message to every subscriber of a given topic
        /// </summary>
        /// <param name="topicName"></param>
        /// <param name="message"></param>
        public void Publish(string topicName, Message message)
        {
            var topic = GetTopic(topicName);
            foreach (var queue in topic.Values)
            {
                queue.Enqueue(message);
            }
        }

        /// <summary>
        /// Grab messages for a given subscription
        /// </summary>
        /// <param name="subscriber">id of the subscriber</param>
        /// <param name="topicName">Case-insensitive name of the topic</param>
        /// <param name="maxMessages">maximum number of messages, default is 10</param>
        /// <returns>an array of messages, possibly empty</returns>
        [Command]
        public Message[] Poll(Guid subscriber, string topicName, int maxMessages = 10)
        {
            var topic = GetTopic(topicName);
            MessageQueue queue;
            if (!topic.TryGetValue(subscriber, out queue)) throw new CommandAbortedException("No such subscriber");
            int count = Math.Min(maxMessages, queue.Count);
            var result = new Message[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = queue.Dequeue();
            }
            return result;
        }

        public String[] GetQueueNames()
        {
            return _queues.Keys.ToArray();
        }

        public String[] GetTopicNames()
        {
            return _topics.Keys.ToArray();
        }

        public Guid[] GetSubscribers(string topicName)
        {
            var topic = GetTopic(topicName);
            return topic.Keys.ToArray();
        }

        public void CreateQueue(string name)
        {
            if (_queues.ContainsKey(name)) throw new CommandAbortedException("Queue already exists");
            _queues[name] = new MessageQueue();
        }

        public void CreateTopic(string name)
        {
            if (_topics.ContainsKey(name)) throw new CommandAbortedException("Topic already exists");
            _topics[name] = new Topic();
        }

        public void DeleteQueue(string queueName)
        {
            if (!_queues.Remove(queueName)) throw new CommandAbortedException("No such queue");
        }

        public void DeleteTopic(string topicName)
        {
            if (!_topics.Remove(topicName)) throw new CommandAbortedException("No such topic");
        }

        public BrokerStatus GetStatus()
        {
            return new BrokerStatus
            {
                Queues = _queues.ToDictionary(ks => ks.Key, vs => vs.Value.Count),
                Topics = _topics.ToDictionary(
                    ks => ks.Key,
                    vs => (IDictionary<Guid, int>) vs.Value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count))
            };

        }

        private Topic GetTopic(string name, bool mustExist = true)
        {
            Topic topic;
            _topics.TryGetValue(name, out topic);
            if (topic == null && mustExist) throw new CommandAbortedException("No such topic: " + name);
            return topic;
            
        }

        private MessageQueue GetQueue(string name, bool mustExist = true)
        {
            MessageQueue queue;
            _queues.TryGetValue(name, out queue);
            if (queue == null && mustExist) throw new CommandAbortedException("No such queue: " + name);
            return queue;
        }
    }
}
