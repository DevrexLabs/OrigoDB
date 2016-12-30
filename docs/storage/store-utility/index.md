---
title: Store Utility
layout: submenu
---
## {{page.title}}
The store utility is a standalone command line executable used to convert between storage formats or upgrade from a previous version.

## Syntax
`$ origodb.storeutility convert <args>`

### Args

*Name* | *Description*
-------|--------------
 `--source` | Path to source store, or name of sql connectionstring in config file
 `--destination` | Path to destination source, or name of sql connectionstring in config file
 `--source-type` | One of `file-v0.4 file-v0.5` or `sql`
 `--destination-type` | file-v0.5 or sql
 `--assembly` | path to model assembly
 `--source-snapshots` | Path to snapshots when source is sql
 `--destination-snapshots` | Path to snapshots when destination is sql

## Example convert file to SQL

```bash
$ origodb.storeutility convert --source=c:\data\freedb --destination=origodbstorage
      --destination-type=sql --source-type=file-v0.5 --assembly=c:\freedb\freedb.core.dll
```

and the file `origodb.storeutility.exe.config` :

```xml
<?xml version="1.0"?>
<configuration>
  <connectionStrings>
    <add name="origodbstorage"
        connectionString="Data Source=.;Initial Catalog=freedb;Integrated Security=True"
        providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```
