---
title: Compiled Linq Queries
layout: submenu
---
## {{page.title}}

The OrigoDb engine can compile and cache parameterized LINQ queries passed as a strings using the following extension methods, defined in the namespace `OrigoDB.Core.Linq`:

```csharp
public object Engine.Execute(string linqQuery, param object[] args);
public R Engine.Execute<M, R>(string linqQuery, param object[] args);
```

The linqQuery must be a valid c# expression. The expression is evaluated with the provided arguments and the result is returned to the caller.

## Parameters
Use `db` within the query to reference the model during execution.
Access the arguments in the `args` param array using `@arg0`, `@arg1`, `@arg2` etc
The type of each parameter is derived from the actual arguments on the first invocation. Subsequent invocations expect arguments that match the signature. Example:

```csharp
class Customer{ public string Name; }
engine.Execute("@arg0.Name", new Customer{Name = "Homer"});
```

Note how the `Customer.Name` property is accessed within the query.

## Example

```csharp
//import extension methods
using OrigoDb.Core.Linq;

var query = "(from Customer c in db.Customers where c.Name.StartsWith(@arg0)).First()"

//call untyped
object untypedResult = engine.Execute(query, "R"); // "R" is params object[]

//call typed
string customerName = engine.Execute<MyModel, string>(query, "H");
```

## Compilation
The query is compiled the first time it is evaluated. The compilation depends on the number and types of arguments passed. Subsequent calls with the same linqQuery must pass the same number and types of arguments.

## Security Warning!
Beware of the risk of arbitrary code injection. There is a test case in the source with an example exploit.
