---
title: Key Value Store
layout: submenu
---
# Key/Value Store

A key value store with case insensitive string keys mapping to versioned objects supporting optimistic concurrency. There are 3 operations:

* `Set(string key, object value, int? expectedVersion = null)`
* `Node Get(string key)`
* `Remove(string key, int? expectedVersion = null)`

Each time a specific key is set the version is incremented, starting with 1. Passing an expected version to `Set` or `Remove` requires that the stored version equals the expected version. Pass zero to `Set` to ensure key is new. If no expected version is passed `Set` will always succeed while `Remove` will succeed if the key exists.

`Get` will throw unless the key exists. The `Node` return type has two fields, `Version` and `Item`. The item is the stored value.

## Example

```csharp
var store = Db.For<KeyValueStore>();
store.Set("mykey",  "myvalue", 0); //expect key doesn't exist
var node = store.Get("mykey");
Assert.AreEqual(node.Version, 1);
Assert.AreEqual(node.Item, "myvalue");

store.Set("mykey", "newvalue"); //no version constraint, increments version from 1 to 2
store.Set("mykey", "fail", 1); //wrong version, expected 1 but was 2
store.Set("mykey", "succeed", 2);

store.Remove("mykey", 2); //FAIL wrong version
store.Remove("mykey");
store.Remove("mykey"); //FAIL, doesn't exist
```

## KeyValueStoreClient
This class wraps a `KeyValueStore` and serializes/deserializes objects to byte array eliminating the need to deploy assemblies with custom types on the remote server.

### Example

```csharp
var store = Db.For<KeyValueStore>("mode=remote");
var client = new KeyValueStoreClient(store, new BinaryFormatter());

//object (graph) to store, must be serializable
var player = new Player("Coder Bob");
client.Set(player.Id, player);

//retrieve object and modify
var node = client.Get(player.Id);
player.AddScore(42);

//write back using optimistic concurrency
client.Set(player.Id, player, node.Version);
```
