---
title: Schema Evolution
layout: submenu
---
## {{page.title}}
Upgrading an existing database can be a challenge whatever type of database you use.
This section describes what kinds of problems can arise when using OrigoDb and how you can deal with them.

## Serialization
The main issue to look out for is serialization exceptions due to serialized objects no longer compatible with a newer type definition. If you rename or remove a
field or rename the type, including changing namespace, you will break compatibility. Adding fields is no problem,
if a field is missing in the deserialization stream, it will be assigned the type default.

## So what gets serialized? 
The command journal contains serialized command objects. Snapshots are serialized model instances. The model is usually a large object
graph containing entities and other custom types. So here's a list of types you need to consider:
* Commands and queries
* Any types referenced by the commands in fields 
* the model itself
* Any types referenced by the model  (for example custom entities)

Remote queries and commands are serialized during transfer. This can become an issue if you use different model versions on the server
and client side or different serialization formats.

## Guidelines
The guiding principle is *serialize as few types as possible*. Obviously, commands need to be serialized, but you can avoid
embedding custom objects within commands. Here are two commands that achieve the same thing. The first example creates a `Task`
during execution, the second example has a field of type `Task`.

### Commands
{% highlight csharp %}
    //Good design
    [Serializable]
    public class AddTaskCommand : Command<MyModel>
    {
       public readonly Guid TaskId;
       public readonly string Name;
       protected override void Execute(MyModel model)
       {
          var task = new Task{Id = TaskId, Name = Name};
          model.AddTask(task);
       }
       //constructor omitted for brevity...
    }
{% endhighlight %}
Bad design, Task object is serialized with the command
{% highlight csharp %}
    [Serialized]
    public class AddTaskCommand : Command<MyModel>
    {
       public readonly Task Task;
       protected override void Execute(MyModel model)
       {
          model.AddTask(Task);
       }
       //constructor omitted for brevity
    }
{% endhighlight %}
If you need to make a breaking change to a command, consider creating a new similar command with a version suffix, eg `AddTaskCommand_v2`.

### Keep a complete journal history and delete snapshots
Snapshots are most often used to speed up load time and can be deleted during upgrade if you have the entire journal preserved.
If you can afford the storage space you should keep the entire journal. 

Another benefit of a complete journal is that you can define entirely new projections over the commands.

### Initializing added fields
*Note* - this applies to the default formatter, BinaryFormatter.

Constructors are not called during deserialization. Initializing added fields has to be done somewhere else.
You can override `Model.SnapshotRestored` or use lazy initialization in a property wrapping the field.
{% highlight csharp %}
    //added field. WRONG, wont get initialized!
    Dictionary<string,Product> _productsByCategory = new Dictionary<string,Product>();

    //better
    Dictionary<string,Product> ProductsByCategory
    {
       get
       {
          if (_productsByCategory == null) _productsByCategory = new _productsByCategory();
          return _productsByCategory;
       }
    }

    //assign and initialize using SnapshotRestored
    protected override void SnapshotRestored()
    {
       //initialize field added in version 4.2
       if (_productsByCategory == null) _productsByCategory = new Dictionary<string,Product>();

       //populate index added in version 4.2
       foreach(_products.SelectMany(p => p.Categories))
       {
          //code excluded in example for brevity...
       }
    }
{% endhighlight %}
## Summary
* Serialize as few types as possible
* Don't rename or remove fields
* Add a lazy property to initialize added fields
* Don't embed custom objects in commands
* Keep a complete journal history
* Delete snapshots during update (requires a complete journal)
* Replace initial snapshot during upgrade
* Add a version suffix to commands
* Implement custom serialization (implement `ISerializable`)
* Use a custom serializer, default is `BinaryFormatter`
