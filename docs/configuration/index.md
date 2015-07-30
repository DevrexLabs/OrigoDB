---
title: Configuration
layout: submenu
---
## {{page.title}}
The engine and it's collaborating components can be configured/customized by passing an instance
of `EngineConfiguration` when creating your engine. The [Extensibility](/docs/extensibility) page
covers what can be customized and how you inject your own custom implementations of the extensible interfaces.

## EngineConfiguration Properties
* **AsynchronousJournaling** - Write commands to the journal in the background. Default is false. Can increase command/query throughput in the short term at the risk of buffered commands getting lost in case of system failure.
* **EnsureSafeResults** - Engine takes responsibility for ensuring no mutable object references are returned by commands or queries. Default is true. Turning it off can yield increased performance (no serialization) but at the risk of data corruption, in case the model is manipulated through a reference returned by a command or query. Temporary corruption can also occur when a subsequent command modifies objects returned which are still in use.
* **CloneCommands** - whether or not to clone commands before executing and journaling, default is true.
* **Location** - Location of the journal files and snapshots if using file storage. Refers to a connection string name or connection string when using Sql Storage.
* **SnapshotLocation** - an alternative snapshot location, defaults to Location
* **StoreType** - Don't set store type directly, inject a `StorageFactory` see [Extensibility](/docs/extensibility)
* **Kernel** - Set what type of kernel to use, see [Kernels](/docs/kernels)
* **ObjectFormatting** - What format to use for serialization
* **PacketOptions** - Set compression and checksum options for command journal packets
* **Synchronization** - Controls which ISynchronizer to use, default is `SingleWriterMultipleReaders`
* **MaxEntriesPerJournalSegment** - Default is 10_000
* **MaxBytesPerJournalSegment** - Default is 8MB