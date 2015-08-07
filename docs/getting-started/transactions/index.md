---
title: Transactions
layout: submenu
---
# {{page.title}}
By default, OrigoDB transactions are serialized providing perfect isolation and the strictest possible consistency. Command and query execution (writes and reads) are the implicit transactional units. The thread safe [Engine](https://github.com/DevrexLabs/OrigoDB/blob/master/src/OrigoDB.Core/Engine.cs) is responsible for proper transaction management. Explicit transactions are not supported, nor are they necessary.

### Atomicity
If a command fails with an unexpected exception, the in-memory model will be restored to the state prior to the failing command.

### Consistency
Serialized isolation guarantees the strongest possible consistency.

### Isolation
Commands are executed sequentially, thus fully isolated. The default `ISynchronizer` uses a `ReaderWriterLockSlim` to allow either a single writer or multiple readers at any given time. This guarantees that reads always see the most recent state (and that the state is not modified) for the entire duration of the transaction. By default, query results are cloned to prevent accidentally returning references to mutable objects within the model.

### Durability
Commands are written and flushed to the journal before execution, also known as write ahead logging.


## MVCC transactions
OrigoDB supports the MVCC concurrency model. With this model, writes are serialized but will not block reads, which use snapshot isolation. Snapshots are based on the most recently committed write transaction. See the section on [Immutability](../../modeling/immutability) for details.

## Explicit transactions?
There are no explicit transactions in OrigoDB. You cannot begin a transaction, make changes, then commit or rollback. Relational databases use the rollback mechanism to resolve conflicting changes caused by concurrent transactions. OrigoDB does not have concurrent transactions so there is no need for explicit transactions. If you need to perform multiple commands as a single atomic unit, consider using the composite command pattern which batches multiple commands in a single transaction:

{% highlight csharp %}
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
{% endhighlight %}

## Performance guidelines

1. Return as little data as possible from queries because cloning is expensive.
1. Return immutable data from queries to avoid result cloning
1. Keep transactions as short as possible for optimal throughput and minimum latency
1. Do expensive preparation or validation in `Command.Prepare()` because it will not block readers
1. If you have  a single threaded process, or control concurrency at the client side, use a [NullSynchronizer](https://github.com/DevrexLabs/OrigoDB/blob/master/src/OrigoDB.Core/Synchronization/NullSynchronizer.cs) and turn off result cloning:
{% highlight csharp %}
var config = new EngineConfiguration();
config.EnsureSafeResults = false;
config.Synchronization = SychronizationMode.None;
var engine = Engine.For<MyModel>(config);
{% endhighlight %}
