---
title: File Storage
layout: submenu
---
## {{page.title}}
By default, the command journal is written to a file system directory.
The journal files are numbered using the scheme `00000000x.00000000y.journal`, where the
first segment is the sequence number of the journal file. The second segment is the id of the first command in the file.

## File Location
The location of the directory is set by assigning to `EngineConfiguration.Location`.
This path is relative to either the current directory or relative App_Data if running in a web context.
If unassigned, Location will default to the class name of the Model.

## Snapshots
Snapshots are written to the same directory as the journal files unless `EngineConfiguration.Location.OfSnapshots` is set to point to somewhere else.

## Example code

```csharp
var config = new EngineConfiguration();
config.Location.OfJournal = "\\Storage\journal";
config.Location.OfSnapshots = "\\Storage\snapshots";
var engine = Engine.For<MyModel>(config);
```
