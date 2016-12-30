---
title: Snapshots
layout: submenu
---
## {{page.title}}
A snapshot is a point in time complete copy of the model written to disk. During restore, the most recent snapshot is read, followed by command replay. Snapshots can be used for backups, to speed up restore or to save disk space (by truncating the journal files).

Snapshots can be taken manually, by calling `Engine.CreateSnapshot()`, or automatically on start or shutdown. Set the property `EngineConfiguration.SnapshotBehavior` before starting the engine:
```csharp
var config = new EngineConfiguration(location);
config.SnapshotBehavior = SnapshotBehavior.OnShutdown;
var engine = Engine.Load<MyModel>(config);
```

## Load Performance
Loading a snapshot is not necessarily faster than replaying all the commands. This is specific to each domain.  Measure before you decide on a snapshot strategy.

## Snapshot naming scheme
Snapshot names have the format `XXXXXXXXX.snapshot`, where XXX is the sequence number of the last command executed just before the snapshot was taken.

## Deleting snapshots
Snapshots are just point in time copies and can safely be deleted as long as you have an intact journal. Discarding snapshots is a trick that can be used during an upgrade if the model has changed (deleted or renamed fields) and has become incompatible with a snapshot based on a previous version.
