---
title: Partition Client
layout: submenu
---
## {{page.title}}
OrigoDb Server, commercially available, supports horizontal data partitioning. `PartitionClusterClient` is the client class used
to communicate with the nodes in the cluster. The client needs to know which nodes to send queries and commands to, either a specific node
or all the nodes. The client also needs to know how to merge results obtained from multiple nodes.

Here's some example code to show how to configure a client.

{% highlight csharp %}
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
       }
    );
{% endhighlight %}

Consider creating a custom client class which derives from PartitionClusterClient and encapsulates the configuration.
Alternatively, define a custom builder class.

## Dispatching
The dispatcher returns either a zero-based index of the target node or an array of node indices for dispatching to multiple nodes.
If you want to dispatch to all nodes, there is no need to register a dispatcher.

## Merging
The merger is a function which merges results from multiple cluster nodes. For a query with the signature `Query<M,R>` a merger must have the signature `Func<R[],R>`.

## Transparent Execution
The `PartitionClusterClient` implements the IEngine interface, use it. The client will transparently take care of dispatching and merging for you.

{% highlight csharp %}
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
{% endhighlight %}


## Partitioning schemes
This example partitions using a modulus function. Adding/removing nodes means all the data needs to be repartitioned.
With this scheme, repartitioning can consume a lot of time, cpu and bandwidth. Also, the client needs to be refreshed with the number of nodes.
To avoid repartitioning you can using a range partitioning scheme. With range partitioning you decide up front on which range of keys each
node will handle. No data needs to be moved between nodes and the client need only be configured once.

## Partitioning and ACID
ACID is guaranteed If and only if every command _mutates_ a single node only. If a command
mutates the state of more than one cluster node, any concurrent multi-node transaction might operate on a mix of old and new states.
Each node maintains its ACID properties but at the cluster level all but durability are jeopardized. Design accordingly.

*Atomicity* - A multi-node command might fail on one or more of the nodes, either due to an exception,
command abortion or system failure. In that case, the client will throw an exception.