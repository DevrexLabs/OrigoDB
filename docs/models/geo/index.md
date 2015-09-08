---
title: Geospatial types
layout: submenu
---
# Geospatial types

Types available for geospatial modeling.

* `GeoPoint` - a point on the surface of a sphere expressed in degrees latitude and longitude.
* `GeoSpatialIndex<T> : IDictionary<T,GeoPoint>` - a dictionary for radius search.
* `ArcDistance` - The length of the shortest path connecting two GeoPoints.

## GeoPoint
The foundational type is the `GeoPoint` representing a point on the surface of a sphere (usually the earth) using Latitude and Longitude coordinates. Use GeoPoint when defining custom entity classes or in conjunction with `GeoSpatialDictionary<T>`.

## ArcDistance
The distance between two points calculated using the haversine formula to an accuracy within 0.5%.

## GeoSpatialIndex<T>
A custom implementation of `IDictionary<T,GeoPoint>`. It uses three SortedSet structures to achieve radius search at O(log N). Use only for radius search, otherwise it's just a waste of memory.

## Examples

The [graph modeling example](../graph/) uses a GeoSpatialIndex.

```csharp
var cheops = new GeoPoint(29.9792345,31.1342019);
var eiffel = new GeoPoint(48.8583701,2.2944813);
ArcDistance d = cheops.DistanceTo(eiffel);
Console.WriteLine("Distance in radians: " d.Radians);
Console.WriteLine("Distance in km: " d.ToKilometers());

var geoIndex = new GeoSpatialIndex<string>();
geoIndex["cheops"] = cheops;
geoIndex["eiffel"] = eiffel;
geoIndex["notre dame"] = new GeoPoint(48.8493659, 2.3386519);
geoIndex.Add("arc de triomphe", new GeoPoint(48.8640344,2.3187821));

//get items within a 5 km radius of the eiffel tower, nearest first
foreach(var pair in geoIndex.WithinRadius(eiffel, 5))
{
  string item = pair.Key;
  ArcDistance d = pair.Value;
  Console.WriteLine("Distance to {0} is {1}", item, d.ToKilometers());
}
```
