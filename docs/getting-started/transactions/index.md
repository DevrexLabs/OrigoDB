---
title: Transactions
layout: submenu
---
# {{page.title}}
OrigoDB transactions are implicit. Commands and queries (writes and reads) are the implicit transactional units.  There are no explicit transactions based on BEGIN, COMMIT and ROLLBACK commands. Instead commands and queries are composed of multiple statements executed as a single isolated transaction. If you have an RDBMS background, you can think of commands and queries as stored procedures.

OrigoDB is designed to achieve the strictest possible consistency by supporting fully ACID transactions. There is no such thing as isolation *level*, transactions are fully isolated. Commands are serialized (executed one at a time) and block queries. Queries run concurrently between commands. (Unless using an `ImmutabilityKernel` which uses snapshot isolation. See MVCC Transactions below)

The [Engine](https://github.com/DevrexLabs/OrigoDB/blob/dev/src/OrigoDB.Core/Engine.cs) is thread safe and manages transactions with the help of `Kernel` and `ISynchronizer` objects.

### Atomicity
`Command` objects are the atomic unit of change. They transition the model from one valid state to the next.
If a command throws an unexpected exception, the model is assumed to be corrupt and will be restored to the state prior to the failing command. A command can fail gracefully by throwing a `CommandAbortedException` which is taken as a guarantee that the model was not altered during execution.

### Consistency
Because commands are serialized and fully isolated from queries the usual anomalies like dirty reads, phantom-reads, non-repeatable reads are non-existent.

### Isolation
There are two types of isolation to consider when using OrigoDB:
* Isolation between transactions
* Isolation of the model from user code when running OrigoDB in-process

Commands are executed sequentially, thus fully isolated. The default `ISynchronizer` uses a `ReaderWriterLockSlim` to allow either a single writer or multiple readers at any given time. This guarantees that reads always see the most recent state (and that the state is not modified) for the entire duration of the transaction.

By default, commands, command results and query results are cloned to prevent leaking references to mutable objects within the model. Cloning uses serialization/deserialization and can have a significant impact on performance. By designing for isolation all or some of the cloning can be disabled. See [Configuration/Isolation](../../configuration/isolation) for details on how to configure what gets cloned and not.

### Durability
Commands are written and flushed to the journal before execution, also known as write ahead logging. A command can not succeed unless it has been persisted to storage. It can however be written to storage and then fail. In this case, if the failure is due to a `CommandAbortedException`, the same failure will occur during replay which is harmless because the model is unaltered. If the exception is of any other type a rollback marker will be appended to the journal causing the failed command to be skipped during replay.


## MVCC transactions
OrigoDB supports the MVCC concurrency model. With this model, writes will not block reads. See the section on [Immutability](../../modeling/immutability) for details.

## Explicit transactions?
There are no explicit transactions in OrigoDB. You cannot begin a transaction, make changes, then commit or rollback. Relational databases use the rollback mechanism to resolve conflicting changes caused by concurrent transactions. OrigoDB does not have concurrent writes so there is no need for explicit transactions. If you need to perform multiple commands as a single atomic unit, consider using the composite command pattern which batches multiple commands in a single transaction.

### Example

```csharp
[Serializable]
public class CompositeCommand<TModel> : Command<TModel>
{
	public readonly Command<TModel>[] ChildCommands;

	public CompositeCommand(IEnumerable Command<TModel> commands)
	{
		ChildCommands = commands.ToArray();
	}

	public override void Execute(TModel db)
	{
		try
		{
			foreach(var command in ChildCommands)
			{
				command.Prepare(db);
				command.Execute(db);
			}
		}
		catch(Exception ex)
		{
		   throw new Exception("A child command threw an exception", ex);
		}
	}
}
```
