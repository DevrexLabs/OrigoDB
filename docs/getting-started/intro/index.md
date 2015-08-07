---
title: Introduction
layout: submenu
---

## What is OrigoDB?
OrigoDB is an in-memory object graph database with an ACID transactional engine at it's heart. Data modeling, commands and queries are written procedurally using any NET language or functionally using LINQ.

The engine, many plugins and data models are all [open source and hosted on github](http://github.com/devrexlabs). OrigoDB Server, offered commercially, is a standalone server with enterprise features like high availability, replication, monitoring and operations.

## Design goals
The initial design goals were focused on rapid development, testability, simplicity, correctness, modularity, flexibility and extensibility. Performance was never a primary goal but running in-memory with memory optimized data structures outperforms any disk oriented system. The primary use case for OrigoDB is handling complex and heavy OLTP workloads in enterprise software.

## Basic concepts
The core component is the `Engine`. The engine is 100% ACID, runs in-process and hosts a user defined data model. The data model can be domain specific or generic and is defined using plain old NET types. Persistence is based on snapshots and write-ahead command logging to the underlying storage.

![image](figure1.png)

### The Model
* is an instance of the user defined data model
* is a strongly typed object graph
* lives in RAM only
* _is_ the data
* is a projection of the entire sequence of commands applied to the initial model, usually empty.
* can only be accessed through the engine

### The Client
* has no direct reference to the model
* interacts directly with the Engine either in-process or remote
* or indirectly via a proxy with the same interface as the model
* passes query and command objects to the engine

### The Engine
* encapsulates the model
* is responsible for atomicity, consistency, isolation and durability
* writes commands to the journal
* executes commands and queries
* reads and writes snapshots
* restores the model on startup

### The Command Journal
* is a complete history of every change to the model
* is the one true source of records
* can be written to any data store

### Commands
* are objects that mutate the model
* are written in C#
* are atomic transactional units
* execute in the same process as the model

### Queries
* are objects that read the model
* are fully isolated from commands
* are written in C#
* are usually based on LINQ
* can be precompiled or dynamically evaluated at runtime

## Modular design
* Modeling - define your own model or use an existing one. Generic or domain specific. It's up to you.
* Storage - File system (default), relational database, Event Store, MongoDB, RavenDB, Azure
* Data format - Choose wire and storage format by plugging in different `IFormatter` implementations. Binary, JSON, ProtoBuf, etc
* Isolation - Serialized transactions, Non-blocking MVCC concurrency using Immutability kernel and model or manage concurrency yourself.

Read more in the docs on [Extensibility](../../configuration/extensibility)
