---
title: Sql Storage
layout: submenu
---
## {{page.title}}


By default, journal files and snapshots are written to the file system. Storage is configurable and supports writing the journal to a relational database taking advantage of existing infrastructure and operations. Also, the journal can be queried/manipulated using regular SQL.

The first step is to set the storage type:

```csharp
var config = new EngineConfiguration();
config.JournalStorage = StorageType.Sql;
```

## The happy path

1. Set the JournalStorage to StorageType.Sql
2. S

## Relevant types
* SqlSettings
* SqlProvider
* MsSqlProvider
* SqlStorage

The provider creates a single table named CommandJournal with the following columns:

Name | Description
-----|------------
Id | The unique sequential id of the command
CommandName | The type name of the command executed
Created | The point in time when the command was executed
Entry | The serialized `JournalEntry<Command>` object

## Supported SQL databases
Release 0.1.0 was tested on Sql Server 2008 R2 developer edition and should work with 2005, 2008, 2012 and SqlCE.

## Configuring SqlStorage
1. Add OrigoDb.Modules.SqlStorage to your project. Grab it on the downloads page or using nuget.
2. Add a connectionstring to your app config file pointing to an existing database
{% highlight xml %}
<connectionStrings>
  <add name="connectionName"
    connectionString="Data Source=.;Initial Catalog=freedb;Integrated Security=True"
    providerName="System.Data.SqlClient" />
</connectionStrings>
{% endhighlight %}

3. Pass an instance of `SqlEngineConfiguration` when creating or loading your database

{% highlight csharp %}
var config = new SqlEngineConfiguration("connectionName");
config.SnapshotLocation = @"c:\\temp";
var engine = Engine.LoadOrCreate<MyModel>(config);
{% endhighlight %}

Alternatively, you can set `Location` to a connection string directly. If so, you must also set the `LocationType` and `ProviderName` properties:

{% highlight csharp %}
var config = new SqlEngineConfiguration();
config.Location = "Data Source=.;Initial Catalog=freedb;Integrated Security=True";
config.LocationType = LocationType.ConnectionString;
config.ProviderName = "System.Data.SqlClient";
config.SnapshotLocation = @"c:\\temp";
var engine = Engine.LoadOrCreate<MyModel>(config);
{% endhighlight %}

## Converting existing journal
Use the StorageUtility to copy an existing journal from file to sql or sql to file.
