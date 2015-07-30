---
title: Child Models
layout: layout
---
## {{page.title}}
**Note! This feature has been removed from version 0.12**
This is a feature which enables reuse of existing models by including them in your Model. Here's a part of the Model class supporting this feature:
{% highlight csharp %}
    [Serializable]
    public class Model
    {
       //keyed by model type
       private Dictionary<Type,Model> _childModels;

       public T ChildFor<T>() where T : Model
       {
          //return a model of type T, creating one if necessary
       }
    }
{% endhighlight %}
And here's an example model (partial) which could be a good candidate for reuse:
{% highlight csharp %}
    [Serializable]
    public class UsersModel : Model
    {
       public void Register(string name, string passwordHash)
       {
          //omitted
       }
       public bool Authenticate(string user, string password)
       {
          //omitted
       }
    }
{% endhighlight %}
Including and accessing the UserModel using a transparent proxy:
{% highlight csharp %}
    //get a proxy for the main model
    var db = Db.For<MyModel>();

    //Get a child proxy and do stuff 
    UsersModel userDb = db.ChildFor<UsersModel>()
    string hash = CryptoHelper.HashFor("DOHnuts");
    userDb.Register("homer", hash);
    bool isAuthenticated = userDb.Authenticate("homer", "DOHnuts");
{% endhighlight %}
Or using traditional command/query style.
{% highlight csharp %}
    [Serializable]
    public class RegisterUserCommand : Command<UsersModel>
    {
       //note how this command targets UsersModel
    }

    IEngine<MyModel> engine = Engine.For<MyModel>();

    //Execute() now accept commands targeted for any model type and routes to child model
    engine.Execute(new RegisterUserCommand("homer", hash));
{% endhighlight %}