---
title: Logging
layout: submenu
---
## {{page.title}}
OrigoDb components produce detailed log information. The core library includes a `ConsoleLogger` which is used by default. You can adjust the amount of detail or disable logging altogether by assigning `ConsoleLogger.MinimumLevel`  with one of the `LogLevel` enumeration values: `Trace, Debug, Info, Warn, Error, Fatal, Off`. The default level is `LogLevel.Info`

## Advanced logging
If you need more sophisticated logging, OrigoDb can be extended to integrate with just about any logging framework. You can implement your own custom module or choose an existing one:

* [log4net](http://logging.apache.org/log4net/)
* [Blackbox](https://github.com/patriksvensson/blackbox)


##  Configuring a custom logging module
1. Add a reference to the logging module, eg `OrigoDb.Modules.BlackBox`.
2. Add module specific configuration to the application configuration file
3. Register your custom `ILoggerFactory` with the `LogProvider` class

using code like this:

```csharp
LogProvider.SetFactory(new BlackboxLoggerFactory());
```

## Implementing a custom logging module
Create a custom logging module by implementing `ILoggerFactory`. Consider deriving from `OrigoDb.Core.Logging.Logger` which can spare you a lot of grunt work, there is only one method that needs to be implemented:

```csharp
protected abstract void Write(LogLevel level, Func<string> messageGenerator);
```

The `ILoggerFactory` has these members:

```csharp
ILogger GetLoggerForCallingType();
ILogger GetLogger(Type type);
```

Origo components call the `GetLoggerForCurrentType()` which should return the full name of the type including the namespace. Most log frameworks can filter and dispatch to different targets based on the name of the logger.

Have a look at the existing modules to learn more.
