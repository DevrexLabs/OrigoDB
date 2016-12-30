---
title: Authorization
layout: submenu
---

Note: Experimental feature and subject to change!

# Authorization

The OrigoDB engine supports role bases authorization for commands and queries based on their type.
By default, every user has full access. To activate authorization, register a custom
`IAuthorizer<Type>` with the EngineConfiguration before starting the engine. The identity is taken from
the current threads `IPrincipal`, normally the account which the process is running as.

Here's an example:

```csharp
   var config = new EngineConfiguration();

   // create an authorizer with a default of denied
   var myAuthorizer = new TypeBasedPermissionSet();

   //add rules for each command/query type, passing an `IEnumerable<string>` with user or role names
   myAuthorizer.Allow<MyCommand<MyModel>>({"Users"});

   //inject a factory lambda that returns the authorizer
   config.SetAuthorizer(cfg => myAuthorizer);

   //Engine picks up and uses the injected authorizer
   Engine.For<MyModel>(config);
```
