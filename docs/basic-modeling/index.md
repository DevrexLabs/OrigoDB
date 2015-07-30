---
title: Basic Modeling
layout: submenu
---

#Basic Modeling
There are many different approaches to modeling with OrigoDb. We try to give you as much freedom as possible. There are just a few constraints:

* You must define a root model class which derives from `OrigoDb.Core.Model`
* The model must be marked with `Serializable`
* Any types referenced by the model must also be marked with `Serializable` (required for snapshots)

One of the core strengths of Origo is that you can choose the most appropriate data representation for your application.
You can do object-oriented domain modeling, relational, key/value, graph, document, column store or whatever else you can think of.
Wan't a 4-dimensional array of Quark-objects? No problem.

Want automatic versioning, snapshots and lock-free concurrency? No problem, build your model using the new awesome immutable collection classes.

We often see people using domain specific object-oriented modeling with either an anemic model and fat transactions or a rich model with thin transactions:

* **Transaction script style** - The model is mainly a data container exposing its data fields, usually collections.
 The domain logic is represented by commands and queries which manipulate the model.
* **Rich domain model** - The model is a facade for your domain exposing behavior in the form of methods while encapsulating data. 
Commands and Queries contain little or no logic, simply mapping properties to arguments passed to methods on the model.
This style plays well with [Transparent Proxy](/docs/proxy) feature.

## Choosing collection classes
You have to consider performance when choosing data structures. Analyzing data access patterns and estimated data volume will help you choose between dictionaries,
lists, hashsets, queues, stacks, arrays etc

Traditional disk-based relational databases use B-trees for tables and indexes.
B-trees provide a reasonable and balanced performance for all types of operations: seek,
scan, insert, delete, etc. `SortedSet<T>`, `SortedList<K,V>` and `SortedDictionary<K,V>` are pretty good
equivalents.
