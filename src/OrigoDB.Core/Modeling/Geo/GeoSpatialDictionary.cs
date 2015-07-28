using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core.Modeling.Geo
{
    /// <summary>
    /// A collection that maps elements of type T to latitude/longitude coordinate pairs.
    /// Useful for finding nearby elements by calling WithinRadius()
    /// </summary>
    [Serializable]
    public class GeoSpatialDictionary<T> : IDictionary<T,LatLon>
    {

        private readonly SortedDictionary<T, Entry> _entries;
        private readonly SortedSet<Entry> _byLatitude;
        private readonly SortedSet<Entry> _byLongitude;

        private class DelegateComparer : IComparer<Entry>
        {
            readonly Func<Entry, Entry, int> _comparer;

            public DelegateComparer(Func<Entry, Entry, int> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(Entry x, Entry y)
            {
                int result = _comparer.Invoke(x, y);
                if (result != 0) return result;
                return Comparer<T>.Default.Compare(x.Item, y.Item);
            }
        }

        [Serializable]
        private class Entry
        {
            public readonly LatLon Point;
            public readonly T Item;

            public Entry(T item, LatLon point)
            {
                Point = point;
                Item = item;
            }

            public Entry(double lat, double lon)
            {
                Point = new LatLon(lat, lon);
            }
        }

        public GeoSpatialDictionary()
        {
            _entries = new SortedDictionary<T, Entry>();
            _byLatitude =
                new SortedSet<Entry>(new DelegateComparer((a, b) => a.Point.Latitude.CompareTo(b.Point.Latitude)));
            _byLongitude =
                new SortedSet<Entry>(new DelegateComparer((a, b) => a.Point.Longitude.CompareTo(b.Point.Longitude)));
        }

        const double EarthCircumference = LatLon.EarthRadiusKm*2*Math.PI;

        /// <summary>
        /// Find all the locations within a given radius of a specified point
        /// </summary>
        /// <returns>Items and their distances to the origin ordered by distance </returns>
        public IEnumerable<KeyValuePair<T, ArcDistance>> WithinRadius(LatLon origin, double radiusInKm)
        {
            //scale radius from km to degrees
            //add 0.5% margin to account for error in distance function
            double radiusInDegreesLatitude = radiusInKm/EarthCircumference*360*1.005;

            var minlat = Math.Max(-90, origin.Latitude - radiusInDegreesLatitude);
            var maxlat = Math.Min(90, origin.Latitude + radiusInDegreesLatitude);

            var south = new Entry(minlat, 0);
            var north = new Entry(maxlat, 0);

            //compensate for shorter distance between meridians
            var absmaxlat = Math.Max(Math.Abs(minlat), Math.Abs(maxlat));
            double distanceInDegreesLongitude = radiusInDegreesLatitude/Math.Cos(absmaxlat * Math.PI / 180);

            var minlon = origin.Longitude - distanceInDegreesLongitude;
            var maxlon = origin.Longitude + distanceInDegreesLongitude;

            return _byLatitude.GetViewBetween(south, north)
                .Intersect(LongitudeRange(minlon,maxlon))
                .Select(entry => new KeyValuePair<T, ArcDistance>(entry.Item, LatLon.Distance(entry.Point, origin)))
                .Where(kvp => kvp.Value.ToKilometers() <= radiusInKm)
                .OrderBy(kvp => kvp.Value);
        }

        
        private IEnumerable<Entry> LongitudeRange(double from, double to)
        {
            if (from < -180)
            {
                var result = _byLongitude.GetViewBetween(new Entry(0, from + 360), new Entry(0,180));
                result.UnionWith(LongitudeRange(-180, to));
                return result;
            }
            if (to > 180)
            {
                var result = _byLongitude.GetViewBetween(new Entry(0, from - 360), new Entry(0, -180));
                result.UnionWith(LongitudeRange(from, 180));
                return result;
            }
            return _byLongitude.GetViewBetween(new Entry(0, from), new Entry(0, to));
        }

        public IEnumerator<KeyValuePair<T, LatLon>> GetEnumerator()
        {
            return _entries.Values.Select(e => new KeyValuePair<T, LatLon>(e.Item, e.Point)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<T, LatLon> item)
        {
            if (_entries.ContainsKey(item.Key)) throw new InvalidOperationException("Key already exists");
            var entry = new Entry(item.Key, item.Value);
            _entries.Add(item.Key, entry);
            _byLatitude.Add(entry);
            _byLongitude.Add(entry);
        }

        public void Clear()
        {
            _entries.Clear();
            _byLatitude.Clear();
            _byLongitude.Clear();
        }

        public bool Contains(KeyValuePair<T, LatLon> item)
        {
            Entry entry;
            _entries.TryGetValue(item.Key, out entry);
            return entry != null && entry.Point == item.Value;
        }

        public void CopyTo(KeyValuePair<T, LatLon>[] array, int arrayIndex)
        {
            foreach (var pair in this)
            {
                array[arrayIndex++] = pair;
            }
        }

        public bool Remove(KeyValuePair<T, LatLon> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public int Count
        {
            get { return _entries.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool ContainsKey(T key)
        {
            return _entries.ContainsKey(key);
        }

        public void Add(T key, LatLon value)
        {
            if (_entries.ContainsKey(key)) throw new InvalidOperationException("Key already exists");
            var item = new Entry(key,value);
            _byLatitude.Remove(item);
            _byLatitude.Add(item);
            _byLongitude.Remove(item);
            _byLongitude.Add(item);
        }

        public bool Remove(T key)
        {
            Entry entry;
            bool result = _entries.TryGetValue(key, out entry);
            if (result)
            {
                _entries.Remove(key);
                _byLatitude.Remove(entry);
                _byLongitude.Remove(entry);
            }
            return result;
        }

        public bool TryGetValue(T key, out LatLon value)
        {
            value = null;
            Entry entry;
            bool result = _entries.TryGetValue(key, out entry);
            if (result) value = entry.Point;
            return result;
        }

        public LatLon this[T key]
        {
            get
            {
                if (_entries.ContainsKey(key)) return _entries[key].Point;
                throw new KeyNotFoundException();
            }
            set
            {
                if (ContainsKey(key)) Remove(key);
                Add(key, value);
            }
        }

        public ICollection<T> Keys
        {
            get { return _entries.Keys; }
        }

        public ICollection<LatLon> Values
        {
            get { return _entries.Values.Select(e => e.Point).ToArray(); }
        }
    }
}