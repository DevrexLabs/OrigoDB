---
title: Relational model
layout: submenu
---
# Relational model
NOTE: This is an experimental feature under development and subject to change!

The relational model enables CRUD style operations based on collections of `IEntity` objects, roughly equivalent to tables in a relational database.
Entities are uniquely identified by Id (Guid) and have an integer Version to support optimistic concurrency.

## Entities
Entities must be isolated object graphs. You may use the Entity base class, a default implementation of `IEntity` for convenience. The base class defines an Id property of type `Guid` and a Version property. The default construct assigns a new guid and lets Version default to 0.

```csharp
[Serializable]
public class Customer : Entity
{
  public string Name{get;set;}
  public List<Address> Addresses {get; private set;}
  public Customer()
  {
    Addresses = new List<Address>();
  }
}
```

## Register types
`bool Create<T>()` - Creates a collection to hold a given entity type, returns true if the collection was created. No-op if the collection already exists. The CRUD methods will throw a `MissingTypesException` if any unregistered entity types are encountered.

## Insert
`void Insert(params[] IEntity entities)` - Insert one or more entitites, entity types can be mixed. If any entity already exists nothing will be modified and an exception will be thrown containing the violating keys.

## Update
`void Update(params[] IEntity entities)` - Update one or more entities by replacing existing entities with matching type, id and version. If there is any mismatch, no changes will be made and an exception will be thrown.

## Delete
`void Delete(params[] IEntity entities)` - Remove one or more entities with matching type, id and version. If there is any mismatch, no changes will be made and an exception will be thrown.

## Get By Id
`T TryGetById<T>(Guid id)` - retrieve an entity by id or null if it doesn't exist. Throws if type is unknown.

## Queries
Write queries as usual using one or more of the following approaches:

* Define a custom model with query methods by sub-classing `RelationalModel`
* Define custom Query classes
* Pass lambda queries to `Engine.Execute` (embedded only)

See [../modeling/queries](Queries) and [../modeling/proxy](Proxy) for details.

## Ad-hoc queries
Using a direct engine reference, call the Execute(Func<M,R> query) overload, passing a lambda

## Versioning
The `Version` property is incremented after a successful insert or update.

## Batch support
By using the `Batch` class, a mix of inserts updates and deletes can be submitted as a single transaction.


## Example batch code

```csharp
var db = Db.For<RelationalModel>();

//Prepare the batch
var batch = new Batch();
batch.Insert(aNewCustomer);
batch.Update(otherCustomer);
batch.Update(someModifiedEntity);
batch.Delete(otherEntity);

try
{
  //Submit the batch
  db.Execute(batch);
}
catch (OptimisticConcurrencyException ex)
{

  var conflicts = ex.Conflicts;
  foreach(EntityKey key in conflicts.Updates)
  {
    Console.WriteLine("Update conflict, Id: " + key.Id +
     ", Version:" + key.Version + ", Type:" + key.Type);
    if (key.Version == 0)
      Console.WriteLine("Attempt to update missing entity");
    else
      Console.WriteLine("Version mismatch");
  }
  //todo: handle delete and insert conflicts
}

```
