---
title: Extensibility
layout: submenu
---
## {{page.title}}
OrigoDb can be extended with custom behavior by choosing between existing or writing your own implementations of one or more of the following interfaces:
* `ILogFactory` - logging module, see [Logging](/docs/logging)
* `ISynchronizer` - Handles reader/writer sychronization, used by the Kernel.
* `IStore` - custom storage provider. `FileStore`, `NullStore` and `SqlStore` are available.
* `ICommandJournal`
* `ISerializer`
* `IFormatter` - Xml, Json, BinaryFormatter (default), ProtoBuf...

###  Example
{% highlight csharp %}
    var config = new EngineConfiguration();
    config.SetStoreFactory(cfg => new MyStorageProvider(cfg));
    _db = Db.For<MyModel>(config);
{% endhighlight %}
When the engine requests an instance of `IStore`, the injected custom constructor function is invoked,
overriding the default `FileStore` implementation. Similar injection methods are available for the other interfaces are available.
