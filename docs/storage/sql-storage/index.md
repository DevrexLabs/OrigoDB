---
title: Sql Storage
layout: submenu
---
## {{page.title}}


Storage is configurable. By default, journal files and snapshots are written to the file system. Using Sql Storage, the journal can be stored in a relational database, allowing you to take advantage of existing infrastructure and keep your DBA's and sysadmins calm. Also, the journal can be queried/manipulated using regular SQL.

Sql storage is flexible and extensible. You can supply custom statements for initializing, reading and writing entries. For custom behavior, create a subclass of  `SqlCommandStore` or `CommandStore`.

The default table has these columns:

Name | Type | Description
---- | ---- | -----
Id | ulong | The unique sequential id of the journal entry
Created | DateTime | When the command was executed
Type | String | The type name of the command executed
Data | Binary  | The serialized command  

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

Here are the default settings. Simply assign new values to customize. The `ConnectionString` property can be assigned either a connection string name in the application configuration file or an actual connection string.

```csharp
config.SqlSettings.TableName = "OrigoJournal";
config.SqlSettings.ConnectionString = "origo";
config.SqlSettings.ProviderName = "System.Data.SqlClient";
config.SqlSettings.SkipInit = false;
var engine = Engine.For<MyDb>(config);
```

## Custom SQL statements
Each supported provider has an associated `SqlStatements` object. The default is `MsSqlStatements` which works with Microsoft Sql Server 2008 and above. To customize, there are a few alternative paths:

* Create an instance of `SqlStatements` and assign each statement.
* Create an instance of a sub class, for example `MsSqlStatements`, and modify existing statements.
* Set a flag causing the `InitStore` statement to be skipped.

To apply custom statements, assign to the `SqlSettings.Statements` property. Example code:

```csharp
var config = new EngineConfiguration();
var settings = config.SqlSettings;
settings.SkipInit = true;
settings.Statements = new SqlStatements
{
  AppendEntry = "EXEC uspAppendEntry @id, @created, @type, @data",
  ReadEntries = "SELECT * FROM {0} WHERE id >= @id ORDER BY id"
}
```

Note the `{0}` placeholder. `String.Format` will be applied to the statements passing a single argument of `SqlSettings.TableName`.

## Supported providers
The ProviderName must be one the following supported providers. Otherwise a custom provider needs to be configured. See Custom Providers below.

* `System.Data.SqlClient` - has been tested with Sql Server 2008 and above
* `System.Data.OleDb` - not yet tested so probably not operational.
* `System.Data.SqlServerCe.4.0` - will use `MsSqlStatements` but has not been tested

##  Custom Providers
The Sql Storage implementation is generic and relies on ADO.NET Provider factories to supply vendor specific connections, commands and parameter objects. Here's some reading on the topic:
https://msdn.microsoft.com/en-us/library/ms379620%28VS.80%29.aspx

Unfortunately, sql differs across vendors so to be able
to use a custom provider, a `SqlStatements` object with compatible statements needs to be associated with the provider name. A fictive example:

```csharp
//Note, this is just an example
//the NpgSqlStatements class does not exist!
SqlCommandStore.ProviderStatements["NpgSql"] = new NpgSqlStatements();
```

## Customizing behavior
Derive from `SqlCommandStore` or `CommandStore` and override one or more methods. Then register the custom store:

```csharp
var config = new EngineConfiguration();
config.SetCommandStoreFactory(cfg => new MySqlCommandStore(cfg));
var engine = Engine.For<MyModel>(config);

```
