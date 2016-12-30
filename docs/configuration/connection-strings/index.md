---
title: Connection Strings
layout: submenu
---
## {{page.title}}

### Example configuration file

```xml
<connnectionStrings>
   <add name="MyModel" connectionString="mode=embedded;location=c:\db\mymodel"/>
   <add name="test" connectionString="mode=remote;host=10.0.0.20;port=3001"/>
 </connnectionStrings>
```

* `Engine.For<MyModel>()` will  create a LocalEngineClient based on the connection string named "MyModel"
* `Engine.For<MyModel>("test")` will create a RemoteEngineClient based on the connection string named "test"

### Local connection string parameters
When mode is "embedded" the connection string properties are mapped to an instance of `EngineConfiguration`.
See [EngineComfiguration](../engine) for a full list of properties, only a few are shown below. When mode is remote, the properties will be mapped to an instance of  `RemoteClientConfiguration`

*Name* | *Type* | *Default* | *Description*
-------|--------|-----------|--------------
location | string | empty string | file path or sql server connection string name
CloneCommands| bool | true |clone commands before execution using default serializer
EnsureSafeResults | bool | true | turn off and engine will assume you know what you're doing
host | string | localhost | ip or hostname of the remote server
port | ushort | 3001 | The port of the remote server
MaxConnections | int | 10 | Maximum number of tcp connections used by the connection pool
DedicatedPool | bool | false | Set to true and the client will create it's own private connection pool
