---
title: Writing Queries
layout: submenu
---

# {{page.title}}

There are a few different kinds of queries:

1. Queries derived from [Query<TModel,TResult>](https://github.com/DevrexLabs/OrigoDB/blob/master/src/OrigoDB.Core/Transactions/Query%5BM%2CR%5D.cs)
2. Lambda queries
3. Runtime compiled queries, see [Compiled Linq queries](../compiled-linq-queries)
4. Implicit queries when proxying, see [Proxy](../proxy)

The corresponding `Engine.Execute` overloads for executing queries are:

```csharp
TResult Engine.Execute<TModel, TResult>(Query<TModel, TResult> query)
TResult Engine.Execute<TModel, TResult>(Func<TModel, TResult> query)
TResult Engine.Execute<TModel, TResult>(string csharpCode, param object[] args)
```

Implicit proxied queries (case #4 above) are translated to [ProxyQuery<TModel, TResult>](https://github.com/DevrexLabs/OrigoDB/blob/master/src/OrigoDB.Core/Proxy/ProxyQuery.cs) objects which derive from [Query<TModel, TResult>](https://github.com/DevrexLabs/OrigoDB/blob/master/src/OrigoDB.Core/Transactions/Query%5BM%2CR%5D.cs) thus fall under case #1.

## Queries derived from Query<TModel, TResult>
This is the preferred way of defining queries. In the derived class, write your query by overriding Execute, which accepts the in-memory model and returns the result. It can be as simple as looking up an object by key or some arbitrarily complex calculation.

Two example queries:

```csharp
// The in-memory model
[Serializable]
public class MyModel : Model
{
   public Dictionary<int,Customer> Customers = new Dictionary<int,Customer>();
}

//A parameterized query
[Serializable]
public class CustomerByIdQuery : Query<MyModel, CustomerView>
{
   //query argument
   public readonly int CustomerId;

   public CustomerByIdQuery(int customerId)
   {
      CustomerId = customerId;
   }

   public override CustomerView Execute(MyModel model)
   {
      //Just let it fail if the key doesn't exist
      return Customers[CustomerId].ToView();
   }
}

//a query without parameters
[Serializable]
public class CustomerCountQuery : Query<MyModel, int>
{
   public override int Execute(MyModel model)
   {
      return Customers.Count;
   }
}

//example usage:
CustomerView customerView = engine.Execute(new CustomerByIdQuery(42));
int numCustomers = engine.Execute(new CustomerCountQuery());
```


## Lambda queries

Lambda queries are simply generic functions that take the model as input and return some value. They're suitable for simple queries, ad-hoc querying with scriptcs, LinqPad or the OrigoDB server web ui. Example given the same model as above:

```csharp
var customerView = engine.Execute(db => db.Customers[42].ToView());
```

### Parameterized lambda queries
The example above uses the constant 42 but of course you can use variables, parameters, objects and methods etc from the calling context. Here's an Asp.NET MVC action:

```csharp
private bool IsVip(Customer c)
{
   return c.Orders.Sum(o => o.Value) > 1000000;
}

public ActionResult GetVipCustomers(int regionId)
{
   var customers = engine.Execute(db => db.Customers
	  .Values
	  .Where(c => c.Region.Id == regionId && IsVip(c))
	  .Select(c => new CustomerView(c))
	  .ToArray()
   );
   return View(customers);
}
```

As you can see this can get messy fast, not to mention how difficult it is to test. A pattern which solves this problem is using query functions, functions that take parameters and return lambda queries:

```csharp
public static class Queries
{
   public static Func<MyModel,CustomerView> CustomerById(int customerId)
   {
      return (MyModel db) => db.Customers[customerId].ToView();
   }

   public static Func<MyModel, CustomerView> VipCustomers(int vipLimit, int regionId)
   {
      return (MyModel db) => db.Customers
		.Values
		.Where(c => c.Region.Id == regionId && c.Orders.Sum(o => o.Value) > vipLimt)
		.Select(c => new CustomerView(c))
		.ToArray();
   }
}
```

Example usage:

```csharp
var customerView = engine.Execute(Queries.CustomerById(customerId));
var vipCustomers = engine.Execute(Queries.VipCustomers(regionId, orderThreshold));
```

## Serializability
Mark your query classes with a `Serializable` attribute to ensure they can be passed to a remote origodb server.

Lambdas/expression trees can't easily be sent across the wire. So the `Engine.Execute` overload which takes a Func is not available on the `IEngine` interface returned by `Engine.For<TModel>()`. If you know you have a local engine, cast from  [IEngine](https://github.com/DevrexLabs/OrigoDB/blob/master/src/OrigoDB.Core/IEngine%5BM%5D.cs) to [Engine](https://github.com/DevrexLabs/OrigoDB/blob/master/src/OrigoDB.Core/Engine%5BM%5D.cs) to access the overload.

## Queries and transactions
Queries are fully isolated from commands, the state of the model will not change while the query is executing (unless you change the default ISynchronizer).

## Query result cloning
To protect you from returning references to mutable objects, query results are deep cloned. See [Views](../views)

## Performance
For optimal throughput and low latency, queries (and commands) should execute as quickly as possible. Some guidelines:
* Don't do anything unnecessary within the execute method, do it before or after.
* Tune your models data structures and algorithms for optimal runtime performance.
* Return as little data as possible, minimizing the cost of serialization
* Return immutable results, they will not be cloned
