---
title: Authoring commands
layout: submenu
---
## {{page.title}}

The command is the atomic transactional write operation of OrigoDB. Commands are executed by the engine which passes a reference of the in-memory model to the `Prepare` and `Execute` methods. To gaurantee isolation, commands may only modify the in-memory model during `Execute`.

## Command classes

Derive from one of the following abstract generic command classes:

* `Command<TModel>`
* `Command<TModel, TResult>`
* `ImmutabilityCommand<TModel>`
* `ImmutabilityCommand<TModel,TResult>`

Override the abstract `Execute` method and optionally the virtual `Prepare` method. Use `Prepare` for validation or cpu intensive preparation without blocking readers.

## Repeatable and side-effect free
Commands are executed once when first submitted and once again for each time the model is restored from the journal.
The commands must produce the exact same change on each invocation. The safest way to achieve this is to only take input
from the current state of the model and the commands fields and properties. Using external input like `Random` objects,
calling date or time functions, reading file system, queues, databases etc are all bad.

For the same reason you should not perform any actions other than writing to the model. Sending messages, writing to queues, database, file or sending confirmation emails are all examples of what not to do. Put this type of behavior in an above layer or as a response to the `Engine.CommandExecuted` event, which is never fired during replay.

Commands are written to the journal before execution. So don't modify the command itself during Prepare/Execute expecting the changes to be persisted, they will be lost.

## Don't use DateTime.Now and friends
Often you want to use the current time, for example setting an order date or invoice date.
Calling DateTime.Now from the command will fail because it is not repeatable. The normal procedure is to set a field/property on the command during creation. The field will be recorded in the journal and reused during restore.

For convenience, commands have a Timestamp property. The timestamp is set by the engine just before execution
and the exact same value is used during replay. Read the timestamp during Prepare or Execute instead of DateTime.Now.
This saves a bit of coding and a few bytes in the journal. Calling Timestamp multiple times during execution will yield
the exact same value.


## Multi command transactions
If you need to execute several commands as a single transaction, define a new command class,
perhaps using the composite pattern, and add the multiple commands as children. Execute the child commands from the parent.


## Performance
Keep execution time as short as possible. Do as much processing as possible before passing the command. Consider performing any lengthy setup that depends on data from the model in the `Prepare` method.

## Exceptions and Aborting
The engine will consider the model corrupt for any exception thrown during `Execute` except `CommandAbortedException`
and rollback by performing a full restore. If an unhandled exception occurs, the engine will wrap it in a `CommandFailedException` and throw it back to the client. Any exception during `Prepare` will cause an abort.

Call `Command.Abort()` to abandon a command. The engine will assume the model unaltered, throw the `CommandAbortedException` to the client and proceed with the next transaction.

## Example

{% highlight csharp %}
//Command with result
[Serializable]
public class AddTaskCommand : Command<TodoModel, int>
{
  public readonly string Title;

  public AddTaskCommand(string title)
  {
	   Title = title;
  }

  public override int Execute(TodoModel model)
  {
    int taskId = model.AddTask(new Task(Title));
    return taskId;
  }
}
{% endhighlight %}
