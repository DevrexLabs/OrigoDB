---
title: Proxy
layout: submenu
---

## {{page.title}}
Transparent Proxy is a rapid development feature eliminating the need to author commands and queries. The proxy acts like an instance of your model. It intercepts method calls and maps to command and query objects of type `ProxyCommand` and `ProxyQuery` which are then passed to an instance of `IEngine`.

## Creating the proxy
The easiest way to create a proxy is using `Db.For<T>()`.

```csharp
//create proxy using Db.For
MyModel model = Db.For<MyModel>();
```

Which is actually just short for:

```csharp
var engine = Engine.For<MyModel>();
MyModel proxy = engine.GetProxy();

//call a method on the proxy
proxy.AddReminder("Write more documentation", DateTime.Now.AddDays(1));
```

Here's what's happening under the hood:

* The proxy intercepts the call to AddRemimder
* Creates an instance of `ProxyCommand` containing the method signature and arguments
* Executes the command on an `IEngine<T>` instance owned by the proxy

The exact same thing happens with queries, except `ProxyQuery` is used
```csharp
//query
var reminders = modelProxy.GetRemindersDue();
```

## What get's proxied?
The proxy has the same type as the model, but not all members are supported. The following rules apply:

* All non-private methods are proxied
* ref/out args are not supported
* Properties and indexers (because they are compiled to methods)
* Generic methods
* Overloaded methods
* Params arguments
* optional arguments
* method calls using named arguments

## Commands and Queries
Void methods are interpreted as commands, non-void methods as queries. If a command method is non-void it must be tagged with a `CommandAttribute`, otherwise it will be interpreted as a query.

```csharp
[Command]
public bool Remove(string key)
{
  //implementation omitted
  return keyWasRemoved;
}
```

### Safe results
If your command or query returns results that don't need to be cloned, use the `CloneResult` property:

```csharp
[Query(CloneResult=false)]
public ReminderView[] GetRemindersDue(DateTime dueBy)
{
  return reminders.Where(r => r.Due <= dueBy)
    .Select(r => new ReminderView(r)).ToArray();
}
```

##  Design Considerations
Make sure that method input and output (arguments and return value) is serializable.

Just because it's easy doesn't mean you can pretend every method call is local. Prefer a chunky over a chatty interaction between your client code and proxy.

Remember that objects returned from queries are copies. The following code will not work as intended because the object returned by `GetReminder()` is a copy of the real object.

```csharp
//wrong! modifying a copy of the object
modelProxy.GetReminder(id).SetCompleted();

//bad, CRUD is an anti-pattern
var reminder = db.GetReminder(id);
reminder.SetCompleted();
db.Save(reminder);

//better
model.SetCompleted(id);
```

## Method overloads
Prior to verion 0.18, overloads were not supported. Only method names were used to identify method calls. If you introduce overloads to system with journal entries created prior to v0.18, you must use `IsDefault` on the original method.

```csharp
    [Command(IsDefault=true)]
    public void SetCompleted(int id){...}

    public void SetCompleted(int id, DateTime at){...}
```

## Mapping to command/query types
Commands and queries are mapped to `ProxyCommand` and `ProxyQuery` objects. You can map to user defined types with the MapTo property. The type must have a constructor taking the same arguments as the method.

```csharp
[Command(MapTo=typeof(SetCompletedCommand))]
public void SetCompleted(int id){...}

//constructor in SetCompletedCommand class
public SetCompletedCommand(int id){..}
```

## Excluding methods
To exclude a method from being proxied, add a `NoProxy` attribute:

```csharp
[NoProxy]
public void Clear(){...}
```

# Properties
Properties in NET are just syntactic sugar. Getters and setters are compiled into methods. Everything that applies to methods apply to setters and getters as well. This means you can use the attributes described above on setters and getter of properties and indexers:
`NoProxy`'Can also be applied to a properties getter or setter:

```csharp
//example property
public Node Root
{
  [Query(CloneResult=false, MapTo=typeof(GetRootNodeQuery))]
  get;
  [NoProxy]
  set;
}
```
