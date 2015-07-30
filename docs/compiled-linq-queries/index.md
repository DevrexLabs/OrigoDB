---
title: Compiled Linq Queries
layout: submenu
---
## {{page.title}}
_Note! This is an experimental feature and subject to change!_

An OrigoDb engine can compile and cache parameterized linq queries passed as a strings. 

The following extension methods are defined in the namespace `OrigoDB.Core.Linq`:
{% highlight csharp %}
public object Engine.Execute(string linqQuery, param object[] args);
public TResult Engine.Execute<TModel, TResult>(string linqQuery, param object[] args);
{% endhighlight %}
The linqQuery must be a valid c# expression. The expression is evaluated with the provided arguments and the result is returned to the caller.

## Parameters
Use `db` within the query to reference the model during execution. 
Access the arguments in the `args` param array using `@arg0`, `@arg1`, `@arg2` etc
The type of each parameter is derived from the actual argument passed. Example:
{% highlight csharp %}
    class Customer{ public string Name;}
    engine.Execute("@arg0.Name", new Customer{Name = "Homer"});
{% endhighlight %}
Note how the `Customer.Name` property is used within the query.

## Example
{% highlight csharp %}
    //import extension methods
    using OrigoDb.Core.Linq;

    var query = "(from Customer c in db.Customers where c.Name.StartsWith(@arg0)).First()"
  
    //call untyped
    object untypedResult = engine.Execute(query, "R"); // "R" is params object[]

    //call typed
    string customerName = engine.Execute<MyModel, string>(query, "H");
{% endhighlight %}
## Compilation
The query is compiled the first time it is evaluated. The compilation depends on the number and types of arguments passed. Subsequent calls with the same linqQuery must pass the same number and types of arguments.

## Security Warning!
Beware of the risk of arbitrary code injection. There is a test case in the source with an example exploit.