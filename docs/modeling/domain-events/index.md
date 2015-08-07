---
layout: submenu
title: Domain Events
---
# Domain Events

OrigoDB domain events are an effort to support event driven, reactive designs. Capture domain events to trigger additional external behavior like sending emails, updating read models, sending messages, relaying through web sockets, etc. Zero or more events are produced during command execution. Events are not yet supported for remote connections or when using proxy.

Events are published via the `Model.Events` property, captured by the `Engine` during command execution and exposed through the `Engine.CommandExecuted` event. 


## Modeling domain events

* Events should be immutable - you can't change historical facts
* Event names should be verbs in the past tense
* Events must not expose mutable references to objects within the model
* Events must implement the empty `IEvent` marker interface
* Consider marking events `Serializable`

### An example event

{% highlight csharp %}
[Serializable]
public class CustomerAdded : IEvent
{
   public int readonly CustomerId;
  
   public CustomerAdded(int id)
   {
      CustomerId = id;
   }
}
{% endhighlight %}

## Publishing and subscribing to events
The `Model.Events` property is an object of type `FilteringEventDispatcher` with the following methods:
{% highlight csharp %}

//subscribe to events of type T
void On<T>(Action<T> handler) where T : IEvent
  
//subscribe to events matching a selector
void Subscribe(Action<IEvent> handler, Func<IEvent, bool> selector);
void Subscribe(IEventHandler handler, IEventSelector selector);
  
//all events
void Unsubscribe(Action<IEvent> handler);
void Unsubscribe(IEventHandler handler);
  
  
//call handlers synchronously one at a time ignoring exceptions
void Send(IEvent @event);
{% endhighlight %}

### Subscribe and Send examples
Subscribe to events directly on the model or by handling `Engine.CommandExecuted` events:

{% highlight csharp %}

//an instance of your data model
var db = new MyModel();

//all events
db.Events.Subscribe(e => MyEventHandler(e));

//all events of a specific (or derived) type
db.Events.On<CustomerCreated>(e => Console.WriteLine(e));

//filtered events
db.Events.Subscribe(e => MyEventHandler(e), e => MyFilter(e));

//publish events
db.Events.Send(new CustomerCreated(42));

//subscribe to events through engine
var engine = Engine.For<MyModel>();
engine.CommandExecuted += (s,e) => {
   foreach(IEvent evt in e.Events)
   {
      //process event
   }
};

//execute command which produces events
engine.Execute(new CreateCustomerCommand(42, "Batman"));


{% endhighlight %}

## Patterns for producing events
Any code with a reference to the model can publish events by calling `Model.Events.Send()`. The two obvious places are from within the model or within `Command.Execute`. Here are examples of both:

{% highlight csharp %}
[Serializable]
public class CreateCustomerCommand : Command<MyModel>
{
   public int Id{get;set;}
   public string Name{get;set;}
   
   public override void Execute(MyModel db)
   {
      var customer = new Customer(Id, Name);
      db.Customers.Add(customer);
      db.Events.Send(new CustomerCreated(Id));
   }
}
//Or from within the model
[Serializable]
public class MyModel : Model
{
   List<Customer> _customers = new List<Customer>();
   
   public void AddCustomer(int id, string name)
   {   var customer = new Customer(id,name);
      _customers.Add(customer);
      Events.Send(new CustomerCreated(id));
   }
}

{% endhighlight %}

## Unsubscribing
The FilteringEventDispatcher uses the subscribed handler and delegate objects as keys. To unsubscribe you must pass the same object used to subscribe. Subscriptions registered using the On<T> cannot be unsubscribed.
