---
title: Partition Client
layout: submenu
---
## {{page.title}}
Data can be spread across multiple OrigoDb Server nodes, aka sharding or horizontal data partitioning. Partition logic is handled by `PartitionClusterClient`, the process is entirely transparent to the server instances. The client maintains a list of nodes, dispatches queries and commands to one or more nodes and merges results from multiple nodes.

## Dispatching
By default, the client dispatches each command and query to every node. Set a custom dispatcher for command or query type T by calling `SetDispatcherFor<T>(Func<T,int> dispatcher)` or `SetDispatcherFor<T>(Func<T,int[]> dispatcher)`. The dispatcher must return either a zero-based index of the target node or an array of node indices for dispatching to multiple nodes.

## Merging
Results from commands and queries need to be aggregated. Register a merger function by calling `SetMergerFor<T>()` where T can be a command, query or result type. The function takes an array of the expected result type and must return an instance of the return type. For a query with the signature `Query<M,R>` a merger must have the signature `Func<R[],R>`. A merger with the wrong signature will throw a runtime exception.

## Example code

```csharp
var client = new PartitionClusterClient<TaskModel>();
var node0 = Engine.For<TaskModel>("mode=embedded;host=node1");
var node1 = Engine.For<TaskModel>("mode=embedded;host=node2");

//add nodes to the partition cluster client, usually these would be instances of `IRemoteEngine`
client.Nodes.Add(node0);
client.Nodes.Add(node1);

// Set dispatchers for each command and query type, default is to send to all nodes
client.SetDispatcherFor<AddTaskCommand>(command => command.Id % 2);
client.SetDispatcherFor<GetTaskByIdQuery>(query => query.Id % 2);

//Register merger functions by return type
client.SetMergerFor<TaskView[]>(Func<TaskView[][], TaskView[]> taskViews => {
  var list = new List<TaskView>();
  foreach(TaskView[] nodeResult in taskViews) list.AddRange(nodeResult);
  return list.ToArray();
});

//merger for query type takes precedence over merger for result type
client.SetMergerFor<TopTenTasksQuery>(
  Func<TaskView[][], TaskView[]> taskViews =>
  {
    var list = new List<TaskView>();
    foreach(TaskView[] nodeResult in taskViews) list.AddRange(nodeResult);
    list.Sort(t => t.DueBy);
    return list.Reverse().Take(10).ToArray();
  });
```

## Using the client

```csharp
//execute commands. They will be dispatched to the correct nodes
client.Execute(new AddTaskCommand{
  Id = 1,
  Name="Play squash",
  Categories="Recreation,Health"});

client.Execute(new AddTaskCommand{
  Id = 2,
  Name="Write some code",
  Categories = "Recreation"});

//query gets dispatched to the correct node
var taskView = client.Execute(new GetTaskByIdQuery{Id = 2});

//send query to all nodes, merge results from each node
//using merger registered for the type TaskView[]
var query = new GetTasksByCategoryQuery("Recreation");
var tasksByCatetory = client.Execute<TaskModel, TaskView[]>(query);
```

## Partitioning schemes
The example above partitions using a modulus function and changing the number of nodes will affect the partitioning. Design a scheme that will accommodate for growth without the need to move data between nodes. For an example, see the [GeekStream model](http://github.com/devrexlabs/geekstream), which is designed for partitioning.

## Partitioning and ACID
When partitioning, atomicity, isolation and consistency are not necessarily guaranteed. Design accordingly.
