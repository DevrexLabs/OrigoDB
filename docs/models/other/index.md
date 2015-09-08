---
title: Other types
layout: submenu
---
# Other types

## Range<T>
Immutable type representing a range of ordered values using upper and lower bounds. Some useful methods:

* `Contains(T value)` - true if value is within the range
* `Precedes(T value)` - true if value > upper bound of range
* `Succeeds(T value)` - true if value < lower bound of range
* `Succeeds(Range<T> other)`
* `Precedes(Range<T> other)`
* `Overlaps(Range<T> other)`
* `Intersect(Range<T> other)` - get the overlapping range, throw if no overlap
* `Union(Range<T> other)` - combine overlapping ranges, throw if no overlap

### Example

```csharp
var reservations = new List<Range<DateTime>>();
reservations.Add(new Range(DateTime.Now, DateTime.Now.AddHours(1)));
var conflicts = reservations.Where(r => r.Overlaps(candidate));
```
