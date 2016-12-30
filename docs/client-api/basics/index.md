---
title: Client API
layout: submenu
---
## {{page.title}}
The client API is used to communicate with in-process or remote OrigoDb instances using a common engine interface, `IEngine<T>`. Switching between in-process and remote can be done using configuration only.

The following classes implement `IEngine<T>`:
* LocalEngineClient
* RemoteEngineClient
* [PartitionClusterClient](../partition-client/) (code-only configuration)

## Creating a client
The recommended way to create a client is by calling `Engine.For<T>()` passing either a connection string, an `EngineConfiguration`, both or nothing at all.

```csharp
//uses the model name, "MyModel", as connection string
IEngine engine = Engine.For<MyModel>();

//when identifier.Contains("=") , interpret as connection string
string location = "mode=remote;host=10.0.0.20;port=1336";
IEngine engine = Engine.For<MyModel>(location);

//location (relative to current working directory) of command/snapshot directory for use wi
IEngine engine = Engine.For<MyModel>("c:\\mymodel");

//if location matches a connection string name in the config file, use the
```

Switching from embedded to remote without rebuilding:

```xml
<connectionStrings>
  <add name="MyModel" connectionString="mode=remote;host=10.0.0.20"/>
</connectionStrings>
```
The identifier string passed to `Engine.For<T>(identifier)` for will be processed according to the following:
1. Connection string name for connection string in app.config / web.config if it exists
1. Connection string if it contains a "="
1. Path where journal files will be stored

The return type of `Engine.For<T>()` is always `IEngine<T>` but the actual type of client returned depends on your configuration. It's always one of the 3 clients listed above.


## IEngine lifecycle
Remote clients have connection pooling built in. When you create a remote client for the first time a number of
tcp connections are pre-opened and pooled. You don't have to manage the connection. Just call `Engine.For<T>()` on a per-call basis.

A local client has similar behavior. The first call to `Engine.For<T>()` will load or create an `Engine<T>` instance in the current process
while subsequent calls will return a reference to the same engine instance. The instance will be Disposed when the process exits. If you need to Dispose earlier you can call `Config.Engines.CloseAll()` or cast to `LocalEngineClient<MyModel>` and grab the engine instance from the Engine property.

```csharp
//Shutdown all local engines
Config.Engines.CloseAll();

//Shutdown specific engine
IEngine client = Engine.For<MyModel>();
(client as LocalEngineClient<MyModel>).Engine.Close();
```

## Transparent Proxy
The client API supports proxying as well. For all of the examples above, `Engine.For<T>` can be replaced with `Db.For<T>` returning a transparent proxy. It's equivalent to the following:

```csharp
var engine = Engine.For<MyModel>();
var db = engine.GetProxy();
```
