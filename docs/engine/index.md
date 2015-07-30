---
title: Engine
layout: submenu
---
## {{page.title}}
The engine:
* encapsulates the database which is an instance of your `Model` class
* executes commands and queries against the database
* synchronizes requests for read and write access to the database

The recommended way to create an engine is through the [client API](/docs/client-api):
{% highlight csharp %}
    var engine = Engine.For<MyModel>();
{% endhighlight %}
`Engine.For<MyModel>()` calls `Engine.LoadOrCreate<MyModel>()`, wraps the returned `Engine` with a `LocalEngineClient` which implements `IEngine<MyModel>`.

The major benefit of using the [Client API](/docs/client-api) is that you can transparently upgrade from embedded engine to remote server
by modifying the application configuration file.

An engine instance can be created directly by calling one of 3 static methods:
* `Engine.Create(...)`
* `Engine.Load(...)`
* `Engine.LoadOrCreate(...)`
each with multiple overloads and generic counterparts.

Most of the overloads for creating and loading take optional string location and `EngineConfiguration` arguments.

## Location
Location of the journal and snapshots. If no location is specified, a default is used. 
If running in a web context the default is `~/App_Data/<model-type-name>`, otherwise
the default is `<current-directory>\<model-type-name>`.

## EngineConfiguration
You might want to use a different location for journal and snapshots or configure the engine for non-default behavior.
If you don't pass an EngineConfiguration, the default, provided by `EngineConfiguration.Create()`, will be used.
{% highlight csharp %}
    var config = EngineConfiguration.Create();
    config.SnapshotLocation = "\\myserver\mysnapshots";
    config.Location = "c:\journal";
    var engine = Engine.Load<MyModel>(config);
{% endhighlight %}
