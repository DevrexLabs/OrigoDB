using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OrigoDB.Core.Models
{
    [Serializable]
    internal class GeoSpatialIndex2<T> : IEnumerable<KeyValuePair<GeoPoint,T>>
    {

        /// <summary>
        /// The number of significant bits to use for the spatial index.
        /// The more bits, the greater the resolution. 52 is about 0.5 meters.
        /// </summary>
        public const int EncodingBitLength = 52;


        class Item : IComparable<Item>
        {
            public ulong Encoding;
            public GeoPoint Point;
            public T Value;
            public int CompareTo(Item other)
            {
                return Encoding.CompareTo(other.Encoding);
            }

            public override bool Equals(object obj)
            {
                return Encoding == ((Item)obj).Encoding;
            }

            public override int GetHashCode()
            {
                return Encoding.GetHashCode();
            }
        }

        readonly SortedSet<Item> _index = new SortedSet<Item>();


        /// <summary>
        /// Calculate number of prefix bits needed for range search
        /// </summary>
        internal static int RadiusToBits(double distanceInKm, double lat)
        {
            const double mercatorMax = 20037.72637;

            if (distanceInKm <= 0) return EncodingBitLength;
            int step = 1;
            while (distanceInKm < mercatorMax)
            {
                distanceInKm *= 2;
                step++;
            }
            step -= 2; /* Make sure range is included in the worst case. */
            /* Wider range torwards the poles... Note: it is possible to do better
             * than this approximation by computing the distance between meridians
             * at this latitude, but this does the trick for now. */
            if (lat > 67 || lat < -67) step--;
            if (lat > 80 || lat < -80) step--;

            /* Frame to valid range. */
            if (step < 1) step = 1;
            if (step > EncodingBitLength / 2) step = EncodingBitLength - 2;
            return step * 2;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        internal static ulong Encode(GeoPoint point, int bits = EncodingBitLength)
        {
            ulong result = 0;
            double minLat = -90, maxLat = 90;
            double minLon = -180, maxLon = 180;

            //take a copy to avoid reading properties multiple times.
            double lat = point.Latitude, lon = point.Longitude;

            while (bits > 0)
            {
                double midLat = (minLat + maxLat) / 2;
                if (lat > midLat)
                {
                    result |= 1;
                    minLat = midLat;
                }
                else maxLat = midLat;

                double midLon = (minLon + maxLon) / 2;
                if (lon < midLon)
                {
                    result |= 2;
                    minLon = midLon;
                }
                else maxLon = midLon;
                result <<= 2;
                bits -= 2;
            }
            return result;

        }
        public void Add(GeoPoint point, T item)
        {
            var entry = new Item
            {
                Point = point, Value = item, Encoding = Encode(point, EncodingBitLength)
            };
            _index.Remove(entry);
            _index.Add(entry);
        }

        public IEnumerable<KeyValuePair<T, double>> WithinDistance2(GeoPoint point, double distanceInKm)
        {
            foreach (var geoPoint in Grid(point, distanceInKm))
            {
                var bits = RadiusToBits(distanceInKm, geoPoint.Latitude);
                
                var prefix = Encode(point, bits);
                var lower = prefix << (EncodingBitLength - bits);
                var upper = (prefix + 1) << (EncodingBitLength - bits);
                foreach (var item in _index.GetViewBetween(new Item{Encoding = lower}, new Item{Encoding = upper}))
                {
                    var distance = item.Point.DistanceTo(point);
                    if (distance <= distanceInKm)
                        yield return new KeyValuePair<T, double>(item.Value, distance);                    
                }
            }
        }

        public IEnumerable<KeyValuePair<T,double>> WithinDistance(GeoPoint point, double distanceInKm)
        {

            ulong prefixMask = GetMask(distanceInKm);
            var seen = new HashSet<T>();
            foreach (GeoPoint geoPoint in Grid(point, distanceInKm))
            {
                Console.WriteLine("gp->" + geoPoint);
                ulong key = Encode(geoPoint);
                ulong low =  key & ~prefixMask;
                ulong high = key | prefixMask;
                Console.WriteLine("{0} {1} {2} {3}", 
                    Convert.ToString((long)key,2), 
                    Convert.ToString((long)low,2), 
                    Convert.ToString((long)high,2),
                    Convert.ToString((long)prefixMask,2));

                var lower = new Item {Encoding = low};
                var upper = new Item {Encoding = high};
                
                foreach (Item item in _index.GetViewBetween(lower, upper))
                {
                    if (seen.Contains(item.Value)) continue;
                    seen.Add(item.Value);
                    var distance = item.Point.DistanceTo(point);
                    if (distance <= distanceInKm)
                        yield return new KeyValuePair<T, double>(item.Value, distance);
                }
            }
        }

        
        internal static ulong GetMask(double distanceInKm)
        {
            double r = GeoLocationSet.EarthCircumference;

            int numBits = 0;
            while (r > distanceInKm && numBits < 52)
            {
                r /= 2;
                numBits += 2;
            }
            return (1UL << numBits) - 1;
        }

        internal IEnumerable<GeoPoint> Grid(GeoPoint point, double distanceInKm)
        {
            double degreesLatitude = distanceInKm/(GeoPoint.EarthRadiusKm * Math.PI * 2) *360;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    var lat = degreesLatitude*i + point.Latitude;
                    if (lat > 90) lat = 90;
                    if (lat < -90) lat = -90;
                    double degreesLongitude = degreesLatitude / Math.Cos(lat);
                    var lon = degreesLongitude * j + point.Longitude;
                    if (lon > 180) lon -= 360;
                    if (lon < -180) lon += 360;
                    
                    yield return new GeoPoint(lat, lon);
                }
            }
            yield return point;
        }

        public IEnumerator<KeyValuePair<GeoPoint, T>> GetEnumerator()
        {
            return (from entry in _index select new KeyValuePair<GeoPoint, T>(entry.Point, entry.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}