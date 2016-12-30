---
title: Isolation
layout: submenu
---

# Configuring Isolation

## Isolated transactions
Transactions (commands and queries) are coordinated using an `ISynchronizer` instance, the default implementation uses an underlying `ReaderWriterLockSlim` allowing a single writer or multiple readers at any given time.

## Isolation of the data model
When running origodb in the same process as your application care must be taken to not leak references to mutablef objects in the database. The following code demonstrates one kind of anomaly that could arise:

```csharp
[Serializable]
public class AddCustomerCommand : Command<MyModel>
{
   public readonly Customer Customer;

   public AddCustomerCommand(Customer customer)
   {
     Customer = customer;
   }
   public void override Execute(MyModel model)
   {
     model.Customers.Add(Customer);
   }
}

//Calling code
var homer = new Customer("Homer");
var command = new MyCommand(homer);
engine.Execute(command);
```

At this point in code, the Customer object is a node in the in-memory object graph and we have a direct reference to it via the homer variable! Unless the object is immutable, we can see changes made to it by other transactions or we could modify it directly rendering the model inconsistent. This would clearly break isolation. The same problem arises if a query/command returns a mutable reference to an existing object.

Fortunately, OrigoDB protects you from reference leaks by making copies of commands (input) and return values (output) unless the types themselves make isolation promises. Note that queries do not change the model, thus their input never needs to be copied, they are input isolated by principle. There are a number of measures you can take to avoid the overhead of serialization and deserialization, which is the mechanism used to copy transaction input and output.

* Design ALL commands and queries to guarantee isolation and turn off copying globally.
* Design as many as transactions as possible/necessary to guarantee isolation and make them known to origodb.

Here are different methods to achieve isolation:
* Make commands and return types immutable and mark with ImmutableAttribute
* Make commands and return types isolated and mark with IsolationAttribute
* Pass primitive types as input and construct objects during execution (see example above and below)
* Make copies without serialization by using mapping, see [Modeling/Views](../../modeling/views) for an example

Here's an isolated equivalent to the `AddCustomerCommand` above:
```csharp
[Immutable]
public class AddCustomerCommand : Command<MyModel>
{
   public readonly string Name;

   public AddCustomerCommand(string name)
   {
     Name = name;
   }

   public void override Execute(MyModel model)
   {
     var customer = new Customer(Name);
     model.Customers.Add(customer);
   }
}
```

The command has been marked with an ImmutableAttribute. Note that immutability guarantees isolation even if the application were to have a leaked reference.

## Configuring Isolation
Here's how to turn off copying globally:

```csharp
var config = new EngineConfiguration();
config.Isolation.Commands = CloneStrategy.Never;
config.Isolation.ReturnValues = CloneStrategy.Never;
```
The two other predefined strategies are CloneStrategy.Always and CloneStrategy.Heuristic. The Always strategy is useful during development to ensure correct behavior when running remote in production. It will catch silly but annoying mistakes like forgetting to add the Serializable attribute to return type. The heuristic strategy is describe in the next section.

## CloneStrategy.Heuristic
The default CloneStrategy for both Commands and ReturnValues is CloneStrategy.Heuristic. When applied to commands, they will be cloned unless:

* The command is marked with the ImmutableAttribute
* The command is marked with the IsolationAttribute(IsolationLevel=Input)

When applied to return values (from both commands and queries), they will be cloned unless:

* The command or query which returns the results are marked with the IsolationAttribute(Level=IsolationLevel.Output)
* The result type itself is marked with ImmutableAttribute
* The result type has been added to the `EngineConfiguration.IsolatedTypes` collection.

The latter is useful for types known to be isolated but for which you don't have control over the source and can add attributes. The most common primitive value types (int,long, float, etc) and other immutable types in the NET Framework are also recognized as isolated: string, object, DateTime, TimeSpan, Guid, TimeZone, DateTimeOffset, Uri and Version.
