using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace OrigoDB.Core.Modeling.Messaging
{
    /// <summary>
    /// Message broker supporting any number of queues (competing consumers)
    /// or buses (multiple subscribers)
    /// </summary>
    [Serializable]
    public class MessageBroker : Model
    {
        [Serializable]
        class MessageQueue : Queue<Message> { }
        
        /// <summary>
        /// A bus is simply a set of subscribers, each with it's own message queue
        /// </summary>
        [Serializable]
        class Bus : Dictionary<Guid, MessageQueue> { }

        private readonly Dictionary<String, MessageQueue> _queues 
            = new Dictionary<string, MessageQueue>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<String, Bus> _buses
            = new Dictionary<string, Bus>(StringComparer.OrdinalIgnoreCase);

        public void Subscribe(Guid subscriber, String busName)
        {
            var bus = GetBus(busName);
            if (bus.ContainsKey(subscriber)) throw new CommandAbortedException("Already subscribed");
            bus[subscriber] = new MessageQueue();
        }

        /// <summary>
        /// Remove subscriber from bus and return any remaining messages
        /// </summary>
        [Command]
        public Message[] Unsubscribe(Guid subscriber, String busName)
        {
            var bus = GetBus(busName);
            MessageQueue q;

            if (!bus.TryGetValue(subscriber, out q))
            {
                throw new CommandAbortedException("No such subscriber");
            }
            bus.Remove(subscriber);
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
        /// Leave a message to every subscriber of a given bus
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="message"></param>
        public void Publish(string busName, Message message)
        {
            var bus = GetBus(busName);
            foreach (var queue in bus.Values)
            {
                queue.Enqueue(message);
            }
        }

        /// <summary>
        /// Grab messages for a given subscription
        /// </summary>
        /// <param name="subscriber">id of the subscriber</param>
        /// <param name="busName">Case-insensitive name of the bus</param>
        /// <param name="maxMessages">maximum number of messages, default is 10</param>
        /// <returns>an array of messages, possibly empty</returns>
        [Command]
        public Message[] Poll(Guid subscriber, string busName, int maxMessages = 10)
        {
            var bus = GetBus(busName);
            MessageQueue queue;
            if (!bus.TryGetValue(subscriber, out queue)) throw new CommandAbortedException("No such subscriber");
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

        public String[] GetBusNames()
        {
            return _buses.Keys.ToArray();
        }

        public Guid[] GetSubscribers(string busName)
        {
            var buses = GetBus(busName);
            return buses.Keys.ToArray();
        }

        public void CreateQueue(string name)
        {
            if (_queues.ContainsKey(name)) throw new CommandAbortedException("Queue already exists");
            _queues[name] = new MessageQueue();
        }

        public void CreateBus(string name)
        {
            if (_buses.ContainsKey(name)) throw new CommandAbortedException("Bus already exists");
            _buses[name] = new Bus();
        }

        public void DeleteQueue(string queueName)
        {
            if (!_queues.Remove(queueName)) throw new CommandAbortedException("No such queue");
        }

        public void DeleteBus(string busName)
        {
            if (!_buses.Remove(busName)) throw new CommandAbortedException("No such bus");
        }

        public BrokerStatus GetStatus()
        {
            return new BrokerStatus
            {
                Queues = _queues.ToDictionary(ks => ks.Key, vs => vs.Value.Count),
                Buses = _buses.ToDictionary(
                    ks => ks.Key,
                    vs => (IDictionary<Guid, int>) vs.Value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count))
            };

        }

        private Bus GetBus(string name, bool mustExist = true)
        {
            Bus bus;
            _buses.TryGetValue(name, out bus);
            if (bus == null && mustExist) throw new CommandAbortedException("No such bus: " + name);
            return bus;
            
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
