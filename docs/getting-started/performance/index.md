---
layout: submenu
title: Performance
---

# Performance
OrigoDB wasn't designed for performance reasons. But running everything in-memory is orders of magnitude faster than moving random pieces of data back forth between disk and memory. Unless commands are very complex and perform a lot of work during execution, command throughput is bound by how fast commands can be written to the journal.

OrigoDB persistence is based on an append-only command journal which is similar to transaction logging in traditional RDBMS systems. The primary differences are that RDBMS systems use *effect logging* and that log records are discarded when no longer needed. During effect logging, new, old, modified and deleted data pages are written to the transaction log. With effect logging, large amounts of data can be written per transaction. This behavior is necessary in order to support transaction rollback. OrigoDB write transactions are not concurrent and thus don't need rollback support. So it is sufficient to only log the *cause* of the transaction, the command object itself. A command in the journal is typically a few hundred bytes.

In a typical RDBMS where the data and log files are on the same disks, checkpoints (dirty pages written to the data files) and queries (require loading data pages from disk) compete with the transaction log writer. In OrigoDB, command logging is the only I/O bound component.

## Transactions per second?
Command throughput is i/o bound and depends on the underlying storage. A few thousand commands per second is normal. (Note. We are designing a new asynchronous engine capable of logging over 100.000 commands per second) During full command load (i/o bound) there is plenty of time to execute queries while waiting for the flush to storage to complete. Query throughput does not involve any disk I/O, it is entirely CPU bound. So the number of queries per second depends varies with size of data and complexity of the query. On a benchmark using a simple Dictionary<int,int> model with Get, Set and Remove commands we measured over 2000 commands per second and 3 million reads per second.

## Custom serialization
Reading commands from the journal during replay is i/o bound. Using a custom serializer that optimizes for size will keep load time as short as possible. Also, the size of journal and snapshots on disk will be kept small. Smaller commands, however, will probably not improve command throughput because they are flushed to storage one at a time. Custom serialization has an additional, non-performance related benefit, easier schema evolution.

## Network performance
When using origodb server, clients pass serialized commands and queries over a tcp connection. Each transaction incurs a single roundtrip. Command and query objects are usually small and fit in a single tcp packet. Keep the size of the results at a minimum for optimal performance.

## Performance guidelines
Guidelines in order of importance:
1. Choose appropriate collection types and algorithms
1. Use a custom serializer, preferably the OrigoDB.Protobuf module.
1. Avoid cloning by using immutable commands and isolated result types
1. Keep transactions as short as possible for optimal throughput and minimum latency
1. Do expensive preparation or validation in `Command.Prepare()` because it will not block readers
