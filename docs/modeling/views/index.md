---
title: Views
layout: submenu
---
## {{page.title}}
Queries that return objects directly from the in-memory model should be avoided. Objects are seldom isolated, they have references to other objects which have references to yet other objects and so on.

For a highly interconnected model, returning an entity or collection of entities will result in a lot of related data being extracted from the model. At worst, the entire model could be returned! The data returned is usually serialized, could contain sensitive information and exposes unwanted behavior and properties in the calling context. One way to address this issue is using views.

## Example
Consider the model below. A `Product` has a reference to one or more `Category` objects. Each `Category` has a list of `Product` objects. A single product contains a reference to 0 or more categories, and each category references a number of products resulting in an object graph.

```csharp
class Product
{
  List<Category> _categories;
}

class Category
{
  List<Product> _products;
}

class ProductModel : Model
{
  List<Category> _categories;
  List<Product> _products;
}
```

Now say we want to display a dropdown list with categories. We want the name and id.
Here's what a beginner might do:

```csharp
//BAD!
var cats = engine.Execute(m => m.Categories.ToArray());
```

It'll work but way too much data is being serialized and returned. Besides the category objects, every Product that has at least one category will also be returned.

You might think the following is a good idea:

```csharp
//runtime exception, anonymous type is not serializable
var cats = engine.Execute(m => m.Categories.Select(c => new {Id=c.Id, Name=c.Name}).ToArray());
```
Here's a better way to do it:

```csharp
//Better?
var cats = engine.Execute(m => m.Categories.Select(c => new CategoryView(c)).ToList());
```

Note how the `CategoryView` constructor takes a `Category` parameter. This is just one way to do it. What's important here is that the mapping is taking place within the Execute-method.

The `CategoryView` class might look something like this:

```csharp
[Serializable]
public class CategoryView : IImmutable
{
  public readonly int Id;
  public readonly string Name;

  public CategoryView(Category category)
  {
    Id = category.Id;
    Name = category.Name;
  }
}
```

Note the IImmutable interface. It will cause the engine to skip serialization because the result and model are isolated.

Ad-hoc lambda queries in application code aren't always desirable, they can become a bit messy,
they only work with an embedded engine and are more difficult to test. So here's another approach using a custom query class. It's cleaner and supports client/server.

```csharp
[Serializable]
public class CategoryViewQuery : Query<MyModel, CategoryView[]>
{
    public CategoryView[] Execute(MyModel model)
    {
      return model.Categories.Select(c => new CategoryView(c)).ToArray();
    }
}

// executing the query...
var cats = engine.Execute(new CategoryViewQuery());
```

### Summary
Returning specific view objects from commands and queries is a good practice with multiple benefits:
* Performance - serialize and return only the data needed
* Security - return only the data needed without exposing sensitive information
* Encapsulation - hide the details of the domain model
* Simplicity - return only data and behavior needed without exposing complex domain entities
