---
title: Snapshots
layout: submenu
---
## {{page.title}}
A snapshot is a complete static image of the model written to disk at a specific time.
The main purpose of snapshots is to speed up load time. When the engine loads, the most recent snapshot is
loaded, followed by replaying commands issued after the snapshot was taken.

Snapshots can be taken manually, by calling `Engine.CreateSnapshot()`, or automatically on start
or shutdown. Set the property `EngineConfiguration.SnapshotBehavior` before starting the engine:
{% highlight csharp %}
    var config = new EngineConfiguration(location);
    config.SnapshotBehavior = SnapshotBehavior.OnShutdown;
    var engine = Engine.Load<MyModel>(config);
{% endhighlight %}

## Load Performance
Loading a snapshot is not necessarily faster than reexecuting commands. This is specific to each domain.  Measure before you decide on a snapshot strategy.

## Snapshot naming scheme
Snapshot names have the format `XXXXXXXXX.YYYYYYYYY.snapshot`, where YYY is the sequence number of
the last command executed just before the snapshot was taken. XXX is a the file sequence number.

## Deleting snapshots
The journal is your complete history of records and should be backed up accordingly.
Snapshots, on the contrary, are just point-in-time copies of the in-memory model and can be discarded at any time, all except the first one.
Discarding snapshots is a trick that can be used during an upgrade if the model has changed (deleted or renamed fields)
and has become incompatible with a snapshot based on a previous version.
