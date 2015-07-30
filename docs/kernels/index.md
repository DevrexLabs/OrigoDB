---
title: Kernels
layout: submenu
---
## {{page.title}}
The `Kernel` is the engine component responsible for transactional integrity. It has the only reference to the in-memory model and controls all access to it. The Kernel loads, restores, creates, executes commands and queries, reads and writes snapshots. Using the default configuration, all operations on the model are _atomic_, _consistent_ (for well-behaved commands), _isolated_ and _durable_.

There are 3 kernels to choose from. Optimistic (default), Pessimistic (deprecated) and the Royal Food Taster.
{% highlight csharp %}
    //Choosing a kernel
    var config = EngineConfiguration.Create();
    config.Kernel = Kernels.RoyalFoodTaster;
    _db = Db.For<MyModel>(config);
{% endhighlight %}
One task performed by the kernel is rolling back after a failed command. There is (currently) no way to undo the effects of a failed command. The only way is to replace the model with the version just prior to the failure. The different kernel types do this replacement in slightly different ways.

## Optimistic Kernel
The optimistic kernel assumes that commands are well behaved, thus writes them to the journal before executing them. If the command fails then a `RollbackMarker` is written to the journal forcing the failed command to be ignored in the future. Rollback is achieved by doing a full restore from storage!

## Pessimistic Kernel
The pessimistic kernel executes the command before writing to the journal. This means that failed commands never reach the journal. For rollback, a full restore is still necessary. More importantly, to guarantee isolation, the pessimistic kernel keeps the write lock on the model until the command has been persisted. If there are a lot of writes, query throughput will be seriously limited. The optimistic kernel will persist the command before aquiring the write lock allowing reads while waiting for I/O operation to complete. For this reason, the pessimistic kernel will be removed in a coming release.

## Royal Food Taster
A full restore rollback can take time to perform during which all access to the model is blocked. The Royal Food Taster alleviates this problem at the cost of twice as much RAM, a so called space/time trade-off.

The food taster keeps 2 identical copies of the in-memory model where one is designated as the taster. Commands are executed on the taster first. If a command fails, the taster is discarded and replaced with a clone of the real model, without requiring full restore. More importantly, the real model is readable while the taster is being rebuilt.

## Summary
Choose between the optimistic and food taster kernels by benchmarking under realistic workloads.
The food taster works well for smaller models. Deserializing large object graphs (which is what happens when the real model is cloned) can be very slow. 