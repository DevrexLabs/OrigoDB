---
title: Sql Storage
layout: submenu
---
## {{page.title}}


Storage is configurable. By default, journal files and snapshots are written to the file system. Using Sql Storage, The journal can be stored in a relational database, allowing you to take advantage of existing infrastructure and operations. Also, the journal can be queried/manipulated using regular SQL.

Sql Storage uses the DbProviderFactories of NET.
The built-in providers are MsSqlProvider and OleDbProvider.

Sql storage is flexible, you can supply custom statements for initializing, reading and writing entries.

The default table has these columns:

Name | Type | Description
---- | ---- | -----
Id | ulong | The unique sequential id of the journal entry
Created | DateTime | When the command was executed
Type | String | The type name of the command executed
Data | Binary or string | The serialized command  

To enable sql storage set the storage type for journaling to Sql:

```csharp
var config = new EngineConfiguration();
config.JournalStorage = StorageType.Sql;
var engine = Engine.For<MyDb>(config);
```

The default settings assume a connection string entry in the application configuration file named 'origo':

```xml
<connectionString name="origo"
  connectionString="Data Source=.;Initial Catalog=freedb;Integrated Security=True"
  providerName="System.Data.SqlClient"/>
```

The providerName must be one the following supported providers or a custom provider. See Custom Providers below.

* System.Data.SqlClient
* System.Data.OleDbClient
* System.Data.OdbcClient

 Here are the default settings, which can be assigned new values. The `ConnectionString` property can be assigned either a connection string name in the application configuration file or an actual connection string.

```csharp
config.SqlSettings.TableName = "OrigoJournal";
config.ConnectionString = "origo";
config.SqlSettings.ProviderName = "System.Data.SqlClient";
var engine = Engine.For<MyDb>(config);
```

## Register a custom provider
Providers derive from SqlProvider and supply the vendor specific sql statements for reading and writing the journal. Custom providers need to be registered using the name and a constructor function taking a SqlSettings as input:
```csharp
SqlProvider.Register("MyProviderName", settings => new MyProvider(settings));
```

Additionally, the provider name must be recognized by DbProviderFactories, see MSDN documentation.
