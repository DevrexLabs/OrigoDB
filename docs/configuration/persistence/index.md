---
title: Persistence modes
layout: submenu
---

## {{page.title}}

There are 3 different modes of persistence:

* Journaling
* SnapshotPerTransaction
* ManualSnaphots

## Journaling
This is the default. Each command is written to the journal before execution.

### SnapshotPerTransaction
Writes a snapshot after every successful command. No journaling.

### Manual snapshots
No journaling. Nothing is saved until Engine.CreateSnaphot() is called or if
SnapshotBehavior is set to either AfterRestore or OnShutdown

## Example

```csharp
var config = new EngineConfiguration();
config.PersistenceMode = PersistenceMode.SnapshotPerTransaction;
var db = Engine.For<MyModel>(config);
```
