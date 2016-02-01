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

## Deterministic and side-effect free
Commands are executed once when first submitted and once again for each time the model is restored from the journal.
A command must produce the exact same change to the model on each invocation. To guarantee consistent state during replay, only depend on the current state of the model and the command itself (fields). Input from external sources may yield different result during replay.  Using `new Random()`, calling `DateTime.Now`, reading file system, external queues, databases etc are examples of what not to do.

Do not perform any external action within a command or the external action will be repeated during replay. Examples of external actions are sending messages, writing to queues, database, file or socket. Instead, perform the external action after the command has executed or as a response to the `Engine.CommandExecuted` event (or a specific Domain Event), of which neither fire during replay.

Commands are written to the journal before execution. So don't modify the command itself during Prepare/Execute expecting the changes to be persisted, they will be lost.


## Dealing with DateTime.Now
Calling `DateTime.Now` from the command will fail because it is not deterministic. It will return a different value during replay. As an alternative you can use `Execution.Current.Now` which will yield the exact same value during replay. `Execution.Current.Now` is set by the engine just prior to recording the command in the journal.

Another straight-forward solution is to provide the `DateTime` value as a command parameter. The field will be recorded in the journal and reused during restore. Example:

```csharp
[Serializable]
public class MarkOrderDelivered : Command<OrdersModel>
{
  public readonly int OrderId;
  public readonly DateTime Delivered;

  public MarkOrderDelivered(int orderId, DateTime delivered)
  {
    OrderId = orderId;
    Delivered = delivered;
  }

  public override void Execute(OrdersModel model) {
    var order = model.FindOrder(OrderId);
    if (order == null) Abort("No such order");
    order.SetDelivered(Delivered);
  }
}
```

## Multi command transactions
If you need to execute several commands as a single transaction, define a new command class,
perhaps using the composite pattern, and add the multiple commands as children. Execute the child commands from the parent.

## Performance
Keep execution time as short as possible. Do as much processing as possible before passing the command. Consider performing any lengthy setup that depends on data from the model in the `Prepare` method as this uses a read lock thus doesn't block other readers.

## Exceptions and Aborting
The engine will consider the model corrupt for any exception thrown during `Execute` except `CommandAbortedException`
and rollback by performing a full restore. If an unhandled exception occurs, the engine will wrap it in a `CommandFailedException` and throw it back to the client. Any exception during `Prepare` will cause an abort.

Call `Command.Abort()` to abandon a command. The engine will assume the model unaltered, throw the `CommandAbortedException` to the client and proceed with the next transaction.

## Example

```csharp
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
```
