---
title: Graph models
layout: submenu
---
# Graph models

On this page:

* Built-in GraphModel
* QuickGraph library

## GraphModel
GraphModel is a minimal built-in generic graph model similar to Neo4j.

### Example
The example demonstrates a twitter graph. Tweets and users are nodes, actions (tweeted, retweeted, followed, favorited) are edges. Querying uses LINQ on the sets of nodes and edges.

```csharp
[TestFixture]
public class GraphModelTests
{
    private GraphModel _graph;
    private long _user1;

    [SetUp]
    public void Init()
    {
        _graph = new GraphModel();
        _user1 = _graph.CreateNode("user");
        var user2 = _graph.CreateNode("user");
        var tweet = _graph.CreateNode("tweet");
        _graph.CreateEdge(_user1, tweet, "tweeted");
        _graph.CreateEdge(user2, tweet, "retweeted");
        _graph.CreateEdge(_user1, user2, "followed");
        _graph.CreateEdge(_user1, _user1, "followed");
    }
    [Test]
    public void GraphIsSerializable()
    {
        new BinaryFormatter().Clone(_graph);
    }

    [Test]
    public void Queries()
    {
        //how many users have at least one tweet?
        Expression<Func<GraphModel, int>> query =
            g => g.Nodes.Count(n => n.Label == "user" && n.Out.Any(e => e.Label == "tweeted"));
        var result = _graph.Query(query);
        Assert.AreEqual(1, result);

        //What is the maximum number of retweets for any tweet
        query = g => g.Nodes
            .Where(n => n.Label == "tweet")
            .Max(n => n.In.Count(e => e.Label == "retweeted"));
        result = _graph.Query(query);
        Assert.AreEqual(1, result);

        //nodes having self references
        Expression<Func<GraphModel, IEnumerable<GraphModel.Node>>> query2 =
            g => g.Nodes.Where(n => n.Out.Any(e => n.In.Contains(e)));

        var node = _graph.Query(query2).Single();
        Assert.AreEqual(node.Id, _user1);
    }
}
```

## Quickgraph
QuickGraph is an open source graph library packed with features. There are multiple graph representations and plenty of algorithms. Visit [QuickGraph on Codeplex](http://http://quickgraph.codeplex.com/) for details.

### Example code
A complete example demonstrating both [GeoSpatial modeling](../geo/) and Quickgraph. [Download VS2013 project](ShortestPath.zip)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core;
using OrigoDB.Core.Modeling.Geo;
using QuickGraph;
using QuickGraph.Algorithms;

[Serializable]
public class FlightModel : Model
{
    [NonSerialized]
    private AdjacencyGraph<string, Leg> _airports;


    private readonly GeoSpatialIndex<string> _locations
        = new GeoSpatialIndex<string>();

    [Serializable]
    public class Leg : Edge<string>
    {
        public readonly double TicketPrice;
        public Leg(string from, string to, double ticketPrice)
            : base(from, to)
        {
            TicketPrice = ticketPrice;
        }
    }

    public bool IsEmpty
    {
        get { return _airports.IsVerticesEmpty; }
    }


    public void Add(string city, GeoPoint location)
    {
        if (_airports.ContainsVertex(city)) throw new CommandAbortedException("airport already exists");
        _airports.AddVertex(city);
        _locations[city] = location;
    }

    public void Connect(string source, string target, double ticketPrice)
    {
        if (!_airports.ContainsVertex(source)) throw new CommandAbortedException("Unknown source airport");
        if (!_airports.ContainsVertex(target)) throw new CommandAbortedException("Unknown target airport");

        _airports.AddEdge(new Leg(source, target, ticketPrice));
        _airports.AddEdge(new Leg(target, source, ticketPrice));
    }

    /// <summary>
    /// Get shortest path by flying distance
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerable<Leg> GetShortestPathByDistance(string source, string target)
    {
        return GetCheapestPath(source, target, e => DistanceInKm(e.Source, e.Target));
    }

    /// <summary>
    /// get path with least number of legs
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public IEnumerable<Leg> GetShortestPathByLegCount(string from, string to)
    {
        return GetCheapestPath(from, to, e => 1);
    }

    public IEnumerable<Leg> GetCheapestPathByTicketPrice(string from, string to)
    {
        return GetCheapestPath(from, to, e => ((Leg) e).TicketPrice);
    }

    public IEnumerable<Leg> GetCheapestPath(string from, string to, Func<Edge<string>, double> costFunction)
    {
        var dijkstra = _airports.ShortestPathsDijkstra(costFunction, from);
        IEnumerable<Leg> path;
        if (dijkstra.Invoke(to, out path)) return path;
        throw new Exception(String.Format("No path from {0} to {1}", from, to));
    }

    /// <summary>
    /// Get at least the 5 nearest airports
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public KeyValuePair<string,ArcDistance>[] MayDay(GeoPoint point)
    {
        double radius = 500;
        while (true)
        {
            var result = _locations.WithinRadius(point, radius).ToArray();
            if (result.Length >= 5) return result;
            radius += 500;
        }
    }

    private double DistanceInKm(string from, string to)
    {
        return _locations[from].DistanceTo(_locations[to]).ToKilometers();
    }

    protected override void SnapshotRestored()
    {
        //hack because AdjacencyGraph is not serializable
        _airports = new AdjacencyGraph<string, Leg>();
    }
}

class Program
{
    static void Main(string[] args)
    {
        var config = new EngineConfiguration();
        config.EnsureSafeResults = false;
        var engine = Engine.For<FlightModel>(config);
        var flightModel = engine.GetProxy();
        if (flightModel.IsEmpty)
        {
            Populate(flightModel);
        }

        var cheapest = flightModel.GetCheapestPathByTicketPrice("HNL", "ARN");
        double price = 0;
        foreach (var leg in cheapest)
        {
            Console.WriteLine(leg + ", price: " + leg.TicketPrice);
            price += leg.TicketPrice;
        }
        Console.WriteLine("Total price: " + price);

        //Point near LHR
        var airports = flightModel.MayDay(new GeoPoint(52, -1.5));
        foreach (var keyValuePair in airports)
        {
            Console.WriteLine("{0} is {1} km away", keyValuePair.Key, keyValuePair.Value.ToKilometers());
        }

        Console.ReadLine();
    }

    static void Populate(FlightModel flightModel)
    {
        flightModel.Add("LAX", new GeoPoint(33.9428575, -118.404631));
        flightModel.Add("PDX", new GeoPoint(45.5897694, -122.5950942));
        flightModel.Add("LHR", new GeoPoint(51.4689508, -0.4548784));
        flightModel.Add("ARN", new GeoPoint(59.6497622, 17.9237807));
        flightModel.Add("CGN", new GeoPoint(50.9572449, 6.9673223));
        flightModel.Add("OSL", new GeoPoint(60.1975501, 11.1004153));
        flightModel.Add("YVR", new GeoPoint(49.1966913, -123.1815123));
        flightModel.Add("CPH", new GeoPoint(55.6180236, 12.6507628));
        flightModel.Add("MUC", new GeoPoint(48.3537449, 11.7860028));
        flightModel.Add("CAI", new GeoPoint(30.1114621, 31.4153838));
        flightModel.Add("LOS", new GeoPoint(6.5818185, 3.3211348));
        flightModel.Add("BKK", new GeoPoint(13.682246, 100.746903));

        flightModel.Add("SIN", new GeoPoint(1.3550328, 103.987668));
        flightModel.Add("HKG", new GeoPoint(22.3087355, 113.915957));
        flightModel.Add("HND", new GeoPoint(35.5493932, 139.7798386));
        flightModel.Add("SFO", new GeoPoint(37.6213129, -122.3789554));

        flightModel.Add("FRA", new GeoPoint(50.0244026, 8.5461304));
        flightModel.Add("CDG", new GeoPoint(49.006986, 2.5536005));
        flightModel.Add("DXB", new GeoPoint(25.2523059, 55.3703183));
        flightModel.Add("HNL", new GeoPoint(21.3212643, -157.9257257));
        flightModel.Add("SEA", new GeoPoint(47.4502499, -122.3088165));

        flightModel.Connect("HNL", "SEA", 289);

        flightModel.Connect("DXB", "SEA", 5173);

        flightModel.Connect("CDG", "DXB", 713);
        flightModel.Connect("CDG", "SEA", 3736);

        flightModel.Connect("FRA", "CDG", 160);
        flightModel.Connect("FRA", "DXB", 668);
        flightModel.Connect("FRA", "SEA", 648);

        flightModel.Connect("SFO", "FRA", 2901);
        flightModel.Connect("SFO", "CDG", 2927);
        flightModel.Connect("SFO", "DXB", 1218);
        flightModel.Connect("SFO", "HNL", 379);
        flightModel.Connect("SFO", "SEA", 151);

        flightModel.Connect("HND", "SFO", 3724);
        flightModel.Connect("HND", "FRA", 2660);
        flightModel.Connect("HND", "CDG", 2994);
        flightModel.Connect("HND", "DXB", 3391);
        flightModel.Connect("HND", "HNL", 1255);
        flightModel.Connect("HND", "SEA", 2021);

        flightModel.Connect("HKG", "HND", 618);
        flightModel.Connect("HKG", "SFO", 1228);
        flightModel.Connect("HKG", "FRA", 1415);
        flightModel.Connect("HKG", "CDG", 1521);
        flightModel.Connect("HKG", "DXB", 1615);
        flightModel.Connect("HKG", "SEA", 1190);

        flightModel.Connect("SIN", "HKG", 136);
        flightModel.Connect("SIN", "HND", 672);
        flightModel.Connect("SIN", "FRA", 1197);
        flightModel.Connect("SIN", "CDG", 1527);
        flightModel.Connect("SIN", "DXB", 531);

        flightModel.Connect("BKK", "SIN", 55);
        flightModel.Connect("BKK", "HKG", 115);
        flightModel.Connect("BKK", "HND", 532);
        flightModel.Connect("BKK", "FRA", 1278);
        flightModel.Connect("BKK", "CDG", 1367);

        flightModel.Connect("LOS", "FRA", 1732);
        flightModel.Connect("LOS", "CDG", 1850);

        flightModel.Connect("CAI", "LOS", 352);
        flightModel.Connect("CAI", "BKK", 773);
        flightModel.Connect("CAI", "FRA", 639);
        flightModel.Connect("CAI", "CDG", 673);
        flightModel.Connect("CAI", "DXB", 538);

        flightModel.Connect("MUC", "CAI", 483);
        flightModel.Connect("MUC", "BKK", 480);
        flightModel.Connect("MUC", "SIN", 1167);
        flightModel.Connect("MUC", "HND", 1209);
        flightModel.Connect("MUC", "SFO", 3581);
        flightModel.Connect("MUC", "FRA", 112);
        flightModel.Connect("MUC", "CDG", 248);
        flightModel.Connect("MUC", "DXB", 1034);

        flightModel.Connect("CPH", "MUC", 184);
        flightModel.Connect("CPH", "CAI", 346);
        flightModel.Connect("CPH", "BKK", 561);
        flightModel.Connect("CPH", "FRA", 169);
        flightModel.Connect("CPH", "CDG", 127);

        flightModel.Connect("YVR", "MUC", 2379);
        flightModel.Connect("YVR", "HKG", 823);
        flightModel.Connect("YVR", "HND", 1006);
        flightModel.Connect("YVR", "SFO", 179);
        flightModel.Connect("YVR", "FRA", 2389);
        flightModel.Connect("YVR", "CDG", 1158);
        flightModel.Connect("YVR", "HNL", 310);
        flightModel.Connect("YVR", "SEA", 161);

        flightModel.Connect("OSL", "CPH", 73);
        flightModel.Connect("OSL", "MUC", 85);
        flightModel.Connect("OSL", "BKK", 502);
        flightModel.Connect("OSL", "FRA", 182);
        flightModel.Connect("OSL", "CDG", 194);

        flightModel.Connect("CGN", "MUC", 98);

        flightModel.Connect("ARN", "CGN", 88);
        flightModel.Connect("ARN", "OSL", 46);
        flightModel.Connect("ARN", "CPH", 52);
        flightModel.Connect("ARN", "MUC", 1056);
        flightModel.Connect("ARN", "CAI", 0);
        flightModel.Connect("ARN", "BKK", 466);
        flightModel.Connect("ARN", "FRA", 185);
        flightModel.Connect("ARN", "CDG", 246);
        flightModel.Connect("ARN", "DXB", 477);

        flightModel.Connect("LHR", "ARN", 96);
        flightModel.Connect("LHR", "CGN", 110);
        flightModel.Connect("LHR", "OSL", 181);
        flightModel.Connect("LHR", "YVR", 2346);
        flightModel.Connect("LHR", "CPH", 114);
        flightModel.Connect("LHR", "MUC", 145);
        flightModel.Connect("LHR", "CAI", 525);
        flightModel.Connect("LHR", "LOS", 1537);
        flightModel.Connect("LHR", "BKK", 580);
        flightModel.Connect("LHR", "SIN", 731);
        flightModel.Connect("LHR", "HKG", 1205);
        flightModel.Connect("LHR", "HND", 1441);
        flightModel.Connect("LHR", "SFO", 2091);
        flightModel.Connect("LHR", "FRA", 145);
        flightModel.Connect("LHR", "CDG", 160);
        flightModel.Connect("LHR", "DXB", 696);
        flightModel.Connect("LHR", "SEA", 2206);

        flightModel.Connect("PDX", "YVR", 183);
        flightModel.Connect("PDX", "SFO", 98);
        flightModel.Connect("PDX", "HNL", 569);
        flightModel.Connect("PDX", "SEA", 106);

        flightModel.Connect("LAX", "PDX", 110);
        flightModel.Connect("LAX", "LHR", 1287);
        flightModel.Connect("LAX", "PDX", 110);
        flightModel.Connect("LAX", "YVR", 116);
        flightModel.Connect("LAX", "CPH", 952);
        flightModel.Connect("LAX", "CAI", 2901);
        flightModel.Connect("LAX", "HKG", 756);
        flightModel.Connect("LAX", "HND", 998);
        flightModel.Connect("LAX", "SFO", 130);
        flightModel.Connect("LAX", "FRA", 2901);
        flightModel.Connect("LAX", "CDG", 3065);
        flightModel.Connect("LAX", "DXB", 1224);
        flightModel.Connect("LAX", "HNL", 185);
        flightModel.Connect("LAX", "SEA", 107);
    }
}
```
