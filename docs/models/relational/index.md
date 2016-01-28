---
title: Relational model
layout: submenu
---
# Relational model
This is an experimental feature under development and subject to change.

The relational model is to enable CRUD style operations based on collections of `IEntity` objects, each uniquely identified by a `Guid`.
Th model supports CRUD operations and  Foreign key references to other entities should be by key and not object reference.

var query = Query.Create(db => db.From<Customer>().First(c => c.Name == "Bilbo"))

class Query<R> : Query<RelationalModel, R>
{

  private RelationalModel _model;

  public IEnumerable<T> Set<T>(){ foreach(T item in _model.ByType(typeof(T)).Values) yield return item;}

    private Query<R>(IExpression<Func<Query<R>,R>> expr){this.Expression = expr;}

  protected R Execute(RelationalModel db)
  {
      return Expression.Invoke(db);
  }

  public static Query<R> Create(IExpression<Func<Query<R>,R>> expr)
  {
    return new Query(expr);
  }
}


rdbms.Query(() => From<Customer>())
db.From<Customer>().

rdbms.Query(() => )

public IEnumerable<T> Set<T>()
{
  foreach(T entity in _entitySets.ByType(typeof(T)).Values)
    yield return entity;
}
## Example code

```csharp

```
