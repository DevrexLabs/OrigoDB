---
title: Extensibility
layout: submenu
---
## {{page.title}}
OrigoDb can be extended with custom behavior by choosing between existing or writing your own implementations of one or more of the following interfaces:
* `ILogFactory` - logging module, see [Logging](../logging)
* `ISynchronizer` - Handles reader/writer synchronization, used by the Kernel.
* `ICommandStore` - custom storage provider for commands. `FileCommandStore` is default. `NullStore` and `SqlStore` are available.
* `ISnapshotStore` - custom storage provider for snapshots. `FileSnapshotStore` is default.
* `IFormatter` - Xml, Json, BinaryFormatter (default), ProtoBuf...

###  Example

```csharp
var config = new EngineConfiguration();
var provider = new MyStorageProvider(); //your customer provider
config.SetCommandStoreFactory(cfg => provider.CreateCommandStore(cfg));
db = Db.For<MyModel>(config);
```
When the engine requests an instance of `ICommandStore`, the injected custom constructor function is invoked, overriding the default `FileCommandStore` implementation. Similar injection methods for the other interfaces are available.

### Formatters
Formatters are used to serialize snapshots, commands, queries, results. Different formatters can be used for the different usages. For reference, here is the FormatterUsage enum and an example showing how to use it.

```csharp
public enum FormatterUsage
{
    Default,
    Snapshot,
    Journal,
    Results,
    Messages
}

//and some code showing usage
var config = new EngineConfiguration();
config.SetFormatterFactory(
  (cfg,formatterUsage) => new MyBsonFormatter(cfg, formatterUsage),
  FormatterUsage.Journal);
```

The default is FormatterUsage.Default meaning the injected constructor will applied to all usages that haven't been explicitly set.
