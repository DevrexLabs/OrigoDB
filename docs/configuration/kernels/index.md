---
title: Kernels
layout: submenu
---
## {{page.title}}
The `Kernel` is the engine component responsible for transactional integrity. It has the only reference to the in-memory model and controls all access to it. The Kernel loads, restores, creates, executes commands and queries, reads and writes snapshots. Using the default configuration, all operations on the model are _atomic_, _consistent_ (for well-behaved commands), _isolated_ and _durable_.

There are 3 kernels to choose from. Optimistic (default), Pessimistic (deprecated) and the Royal Food Taster.

```csharp
//Choosing a kernel
var config = new EngineConfiguration();
config.Kernel = Kernels.RoyalFoodTaster;
db = Db.For<MyModel>(config);
```

One task performed by the kernel is rolling back after a failed command. There is (currently) no way to undo the effects of a failed command. The only way is to replace the model with the version just prior to the failure. The different kernel types do this replacement in slightly different ways.

## Optimistic Kernel
The optimistic kernel assumes that commands are well behaved, thus writes them to the journal before executing them. If the command fails then a `RollbackMarker` is written to the journal forcing the failed command to be ignored in the future. Rollback is achieved by doing a full restore from storage!

## Immutability Kernel
The immutability kernel is used in conjunction with immutable modeling. See [Immutability](../../modeling/immutability) for details.

## Royal Food Taster
A full restore rollback can take time to perform during which all access to the model is blocked. The Royal Food Taster alleviates this problem at the cost of twice as much RAM, a so called space/time trade-off.

The food taster keeps 2 identical copies of the in-memory model where one is designated as the taster. Commands are executed on the taster first. If a command fails, the taster is discarded and replaced with a clone of the real model, without requiring full restore. More importantly, the real model is readable while the taster is being rebuilt.

## Summary
Choose between the optimistic and food taster kernels by benchmarking under realistic workloads. The food taster works well for smaller models. Deserializing large object graphs (which is what happens when the real model is cloned) can be very slow. 
