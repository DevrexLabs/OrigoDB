using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class DomainEventTests
    {
        [Serializable]
        class Event : IEvent
        {
            
        }

        [Serializable]
        class EventEmittingCommand : Command<TestModel>
        {
            /// <summary>
            /// The event to emit
            /// </summary>
            readonly IEvent _event;

            /// <summary>
            /// Number of times to emit the event
            /// </summary>
            readonly int _count;

            public EventEmittingCommand(IEvent @event, int count = 1)
            {
                _event = @event;
                _count = count;
            }
            public override void Execute(TestModel model)
            {
                for (int i = 0; i < _count; i++)
                {
                    model.Events.Send(_event);
                }
                
            }
        }

        [Test]
        public void CanRegisterGenericHandler()
        {
            var target = new FilteringEventDispatcher();
            int calls = 0;
            target.Subscribe(evt => calls++);
            target.Send(new Event());
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void CanUnregisterGenericHandler()
        {
            var target = new FilteringEventDispatcher();
            int calls = 0;
            Action<IEvent> handler = evt => calls++;
            target.Subscribe(handler);
            target.Send(new Event());
            Assert.AreEqual(1, calls);
            target.Unsubscribe(handler);
            calls = 0;
            target.Send(new Event());
            Assert.AreEqual(0, calls);
        }


        [Test]
        public void CanReRegisterGenericHandler()
        {
            var target = new FilteringEventDispatcher();
            int calls = 0;
            Action<IEvent> handler = evt => calls++;
            target.Subscribe(handler);
            target.Subscribe(handler);
            target.Send(new Event());
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void CanFilterBasedOnType()
        {
            var target = new FilteringEventDispatcher();
            int calls = 0;
            target.Subscribe(evt => calls++, evt => evt is Event);
            target.Send(new Event());
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void CanFilter()
        {
            var target = new FilteringEventDispatcher();
            int calls = 0;
            target.Subscribe(evt => calls++, evt => false);
            target.Send(new Event());
            Assert.AreEqual(0, calls);
        }

        [Test]
        public void IgnoresExceptions()
        {
            var target = new FilteringEventDispatcher();
            target.Subscribe(evt => {throw new Exception();});
            target.Send(new Event());
        }

        [Test]
        public void DomainEventsAreDispatched()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest();
            var engine = Engine.Create<TestModel>(config);
            var @event = new Event();
            var events = new List<IEvent[]>();
            engine.CommandExecuted += (s, e) => events.Add(e.Events);
            
            engine.Execute(new EventEmittingCommand(@event));
            engine.Execute(new EventEmittingCommand(@event, 0));
            engine.Execute(new EventEmittingCommand(@event, 2));
            engine.Execute(new EventEmittingCommand(@event, 4));

            //each invocation should produce an array of events
            Assert.AreEqual(4, events.Count);
            CollectionAssert.AllItemsAreInstancesOfType(events, typeof(IEvent[]));
            Assert.AreEqual(1, events[0].Count());
            Assert.AreEqual(0, events[1].Count());
            Assert.AreEqual(2, events[2].Count());
            Assert.AreEqual(4, events[3].Count());
            Assert.IsTrue(events.SelectMany(e => e).All(e => e == @event));

        }

        

    }
}