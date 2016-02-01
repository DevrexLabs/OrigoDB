## In-memory database for NET/Mono

An Origo database is a command-sourced in-memory object graph.

Write your custom data model, commands and queries using any .NET language, or choose from a number of existing generic models.

OrigoDB is developed and maintained by Devrex Labs. We offer commercial support, consulting, training and enterprise features.

To learn more, visit the [project web site](http://dev.origodb.com) and read the [online documentation](http://dev.origodb.com/docs).

[![Join the chat at https://gitter.im/DevrexLabs/OrigoDB](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/DevrexLabs/OrigoDB?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://ci.appveyor.com/api/projects/status/v96t7i3a1kf0gqq3/branch/dev?svg=true)](https://ci.appveyor.com/project/rofr/origodb/branch/dev)

## Example code
```csharp
//Nuget: Install-Package OrigoDb.Core
//Restore graph by replaying commands stored on disk
//in the current working directory
var engine = Engine.For<MyModel>();

//Log a command to disk and then apply to in-memory graph
engine.Execute(new MyCommand{MyArg = someValue});

//read the in-memory graph using a custom query
var results = engine.Execute(new MyQuery{MyArg = someValue});

// -----------------------------------------
// Example #2 - Implicit operations
// -----------------------------------------
//restore graph and wrap the engine in a proxy
MyModel db = Db.For<MyModel>();

//void methods are translated to commands
db.MyCommand(someValue);

//non-void methods are translated to queries
var results = db.MyQuery(someValue);
```

## Key Strengths
* As transparent as persistence can get
* Rapid development, 50% less code on the backend
* High performance, complex transaction processing
* Suitable for Domain Driven Design
* Very easy to unit test and debug
* 100% ACID, fully serialized transactions
* Powerful queries using LINQ
* Flexible and extensible data modeling using modern language and full access to the .NET Framework Class library
* Full audit trail through complete history of commands
* Mature, used in production all over the world
* Command store

## Nuget
`Install-package OrigoDB.Core`

## Links
* Project web site: http://dev.origodb.com
* Documentation: http://dev.origodb.com/docs
* Downloads: http://dev.origodb.com/download
* Release notes: https://github.com/devrexlabs/origodb/releases
* http://twitter.com/devrexlabs
* http://twitter.com/robertfriberg
