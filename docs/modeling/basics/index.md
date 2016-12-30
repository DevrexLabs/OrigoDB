---
title: Basic Modeling
layout: submenu
---

# Basic Modeling
Most databases are based on single data model. Relational, Key/Value, Column store, document, graph etc. OrigoDB is different. You can choose from a number of existing data models, extend an existing model or define your own using any .NET language. The modeling section describes defining your own custom domain model.

An OrigoDB in-memory model is an instance of a single class which derives from `OrigoDb.Core.Model`. Start by creating a class that inherits Model and add fields/properties to represent the state of your system, usually strongly typed collections containing custom entities. Define Entities, Value Types, Enums, Structs, built-in types. Write Command and Query classes to read and write the model.

Just use plain old object oriented design principles.

We often see domain specific object-oriented modeling based on either an anemic model and fat transactions or a rich model with thin transactions:

* **Transaction script style** - The model is mainly a data container exposing its data fields, usually collections. The domain logic is represented by commands and queries which manipulate the model.
* **Rich domain model** - The model is a facade for your domain exposing behavior in the form of methods while encapsulating data. Commands and Queries contain little or no logic, simply mapping properties to arguments passed to methods on the model. This style plays well with the [Proxy](../proxy) feature.

## Choosing collection classes
You have to consider performance when choosing data structures. Analyzing data access patterns and estimated data volume will help you choose between dictionaries, lists, hashsets, queues, stacks, arrays etc

Traditional disk-based relational databases use B-trees for tables and indexes. B-trees provide a reasonable and balanced performance for all types of operations: seek, scan, insert, delete, etc. `SortedSet<T>`, `SortedList<K,V>` and `SortedDictionary<K,V>` are pretty good equivalents.
