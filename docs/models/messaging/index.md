---
title: Message Broker
layout: submenu
---
# Message Broker
The `MessageBroker` class is a model which supports message queues and pub/sub messaging. A queue is a simple FIFO structure.

## Message types
The broker handles messages of type `Message`. Each message has a `Subject`, `Created` and `Id` fields of type `string`, `DateTime` and `Guid` respectively. There are two built-in sub classes, `BinaryMessage` with a byte[] field and `TextMessage` with a string field.

## Queue
A queue is a simple FIFO structure with two operations, `Enqueue` and `Dequeue`. Dequeue will remove and return the oldest message in the queue, or null if the queue is empty. A message can only be consumed by a single client.

## Bus
A bus offers pub/sub messaging. A client can Subscribe to a messages from the Bus by passing the name of the bus and a guid. A dedicated queue will be created for the subscriber holding all the messages published after the subscription was made. Call `Publish` to send a message to all the current subscribers.

Subscriptions are persistent until `Unsubscribe` is called or the Bus to which it is attached is removed.

## Example code

```csharp
var broker = Db.For<MessageBroker>();
broker.CreateQueue("myqueue");
broker.Enqueue("myqueue", new TextMessage("I hate wabbits"));
Message message = broker.Dequeue("myqueue");

//create a bus and subscribe to it
var id = Guid.NewGuid();
broker.CreateBus("mybus");
broker.Subscribe(id, "mybus");

//publish messages
broker.Publish("mybus", message);
broker.Publish("mybus", new TextMessage("More Hello"));

//Get structure names and subscriber/message counts
BrokerStatus status = broker.GetStatus();
foreach(var key in status.Queues.Keys)
  Console.WriteLine("queue: " + key + ", messages: " + status.Queues[key])

//Retreive a batch of up to 10 messages for subscriber, in this example there should be 2
Message[] messages = broker.Poll(id, "mybus");

broker.Unsubscribe(id, "mybus");
broker.DeleteQueue("myqueue");
broker.DeleteBus("mybus");
```
