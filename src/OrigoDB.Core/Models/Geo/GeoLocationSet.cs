using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core.Models
{
    [Serializable]
    public class GeoSpatialIndex<T> : IEnumerable<KeyValuePair<GeoPoint, T>> where T : IComparable<T>
    {
        [Serializable]
        private class DelegateComparer : IComparer<Item>
        {
            readonly Func<Item, Item, int> _comparer;

            public DelegateComparer(Func<Item, Item, int> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(Item x, Item y)
            {
                return _comparer.Invoke(x, y);
            }
        }

        [Serializable]
        private class Item
        {
            public readonly GeoPoint Point;
            public readonly T Value;

            public Item(GeoPoint point, T value)
            {
                Point = point;
                Value = value;
            }

            public Item(double lat, double lon)
            {
                Point = new GeoPoint(lat, lon);
            }
        }

        private readonly SortedSet<Item> _byLatitude, _byLongitude;

        public GeoSpatialIndex()
        {
            _byLatitude =
                new SortedSet<Item>(new DelegateComparer((a, b) => a.Point.Latitude.CompareTo(b.Point.Latitude)));
            _byLongitude =
                new SortedSet<Item>(new DelegateComparer((a, b) => a.Point.Longitude.CompareTo(b.Point.Longitude)));
        }

        public const double EarthCircumference = GeoPoint.EarthRadiusKm*2*Math.PI;

        /// <summary>
        /// Find all the locations within a given radius of a specified point
        /// </summary>
        /// <returns>An ordered list of </returns>
        public IEnumerable<KeyValuePair<T, double>> WithinDistance(GeoPoint origin, double distanceInKm)
        {
            //scale radius from km to degrees
            //add 0.5% margin to account for error in distance function
            double distanceInDegreesLatitude = distanceInKm/EarthCircumference*360*1.005;

            var minlat = Math.Max(-90, origin.Latitude - distanceInDegreesLatitude);
            var maxlat = Math.Min(90, origin.Latitude + distanceInDegreesLatitude);

            var south = new Item(minlat, 0);
            var north = new Item(maxlat, 0);

            var absmaxlat = Math.Max(Math.Abs(minlat), Math.Abs(maxlat));
            double distanceInDegreesLongitude = distanceInDegreesLatitude/Math.Cos(absmaxlat * Math.PI / 180);

            var minlon = origin.Longitude - distanceInDegreesLongitude -20;
            var maxlon = origin.Longitude + distanceInDegreesLongitude +20;

            return //_byLatitude.GetViewBetween(south, north)
                //.Intersect(LongitudeRange(minlon,maxlon))
                LongitudeRange(minlon, maxlon)
                .Select(p => new KeyValuePair<T, double>(p.Value, GeoPoint.DistanceInKm(p.Point, origin)))
                .Where(t => t.Value <= distanceInKm)
                .OrderBy(t => t.Value);
        }

        private ISet<Item> LongitudeRange(double from, double to)
        {
            if (from < -180)
            {
                var result = _byLongitude.GetViewBetween(new Item(0, from + 360), new Item(0,180));
                result.UnionWith(LongitudeRange(-180, to));
                return result;
            }
            if (to > 180)
            {
                var result = _byLongitude.GetViewBetween(new Item(0, from - 360), new Item(0, -180));
                result.UnionWith(LongitudeRange(from, 180));
                return result;
            }
            return _byLongitude.GetViewBetween(new Item(0, from), new Item(0, to));


        }

        public IEnumerator<KeyValuePair<GeoPoint, T>> GetEnumerator()
        {
            return _byLatitude.Select(item => new KeyValuePair<GeoPoint, T>(item.Point, item.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();

        }

        internal void Add(GeoPoint point, T value)
        {
            var item = new Item(point, value);
            _byLatitude.Remove(item);
            _byLatitude.Add(item);
            _byLongitude.Remove(item);
            _byLongitude.Add(item);
        }
    }
}