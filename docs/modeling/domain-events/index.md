---
layout: submenu
title: Domain Events
---
# Domain Events

OrigoDB domain events are an effort to support event driven, reactive designs. Capture domain events to trigger additional external behavior like sending emails, updating read models, sending messages, relaying through web sockets, etc.

Publish events by adding to the List `Execution.Current.Events`.

Zero or more events are produced during command execution. The events are collected by the `Engine` and included in the `EventArgs` when the `Engine.CommandExecuted` event fires. If a command fails or is aborted, no events are published.


## Modeling domain events

* An event is an object representing something that has happened in the past.
* The event name should be a verb in the past tense
* Events should be immutable
* Events must not expose mutable references to objects within the model
* Events must implement the empty `IEvent` marker interface
* Mark events `Serializable`

### An example event

```csharp
[Serializable]
public class CustomerAdded : IEvent
{
   public int readonly CustomerId;

   public CustomerAdded(int id)
   {
      CustomerId = id;
   }
}
```

## Producing events
Emit events by adding them to the current execution context.

```csharp
[Serializable]
public class CreateCustomerCommand : Command<MyModel>
{
   public int Id{get;set;}
   public string Name{get;set;}

   public override void Execute(MyModel db)
   {
      var customer = new Customer(Id, Name);
      db.Customers.Add(customer);
      var ctx = Execution.Current;
      ctx.Events.Add(new CustomerCreated(Id));
   }
}
```

### Consuming events
Subscribe to domain events through `Engine.CommandExecuted`:

```csharp
var engine = Engine.For<MyModel>();
engine.CommandExecuted += (s,e) => {
   foreach(IEvent evt in e.Events)
   {
      Console.WriteLine("Domain Event: " + evt);
   }
};

//execute command which produces events
engine.Execute(new CreateCustomerCommand(42, "Batman"));
```
