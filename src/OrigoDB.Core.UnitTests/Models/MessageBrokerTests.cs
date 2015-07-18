using System;
using NUnit.Framework;
using OrigoDB.Core.Models;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class MessageBrokerTests
    {
        [Test]
        public void SmokeTest()
        {
            const string aQueue = "myqueue";
            const string aTopic = "mytopic";
            const string aGreeting = "Hello world!";
            var aMessage = new TextMessage(aGreeting);


            var config = new EngineConfiguration().ForIsolatedTest();
            var broker = Db.For<MessageBroker>(config);
            
            //create/write/read queue
            broker.CreateQueue(aQueue);
            broker.Enqueue(aQueue, aMessage);
            var message = (TextMessage) broker.Dequeue(aQueue);
            Assert.IsNotNull(message);
            Assert.AreEqual(aGreeting, message.Body);

            //if queue is empty null is returned
            message = (TextMessage) broker.Dequeue(aQueue);
            Assert.IsNull(message);

            broker.CreateTopic(aTopic);
            
            //no op, no subscribers
            broker.Publish(aTopic, aMessage);

            var aSubscriber = Guid.NewGuid();
            broker.Subscribe(aSubscriber, aTopic);

            broker.Publish(aTopic, aMessage);
            broker.Publish(aTopic, aMessage);

            var messages = broker.Poll(aSubscriber, aTopic);
            Assert.AreEqual(messages.Length, 2);

            //Messages are immutable so we should get same instances back!
            Assert.AreSame(messages[0], aMessage);
            
            Guid[] subscribers = broker.GetSubscribers(aTopic);
            Assert.AreEqual(subscribers.Length, 1);
            Assert.AreEqual(aSubscriber, subscribers[0]);

            broker.Enqueue(aQueue, aMessage);
            broker.Enqueue(aQueue, aMessage);
            broker.Enqueue(aQueue, aMessage);
            broker.Publish(aTopic, aMessage);

            var status = broker.GetStatus();
            Assert.AreEqual(status.Queues.Count, 1);
            Assert.AreEqual(status.Topics.Count, 1);

            Assert.AreEqual(status.Queues[aQueue], 3);
            Assert.AreEqual(status.Topics[aTopic].Count, 1, "Expected one subscriber");
            Assert.AreEqual(1, status.Topics[aTopic][aSubscriber]);

            

            broker.Unsubscribe(aSubscriber, aTopic);
            Assert.AreEqual(broker.GetSubscribers(aTopic).Length, 0);
        }
    }
}