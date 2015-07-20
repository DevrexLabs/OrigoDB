using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core.Types;

namespace OrigoDB.Core.Models
{
    [Serializable]
    public class ByLatitude : IComparer<GeoLocation>
    {
        public int Compare(GeoLocation x, GeoLocation y)
        {
            int result = Math.Sign(x.Point.Latitude - y.Point.Latitude);
            if (result == 0) result = String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            return result;
        }
    }

    [Serializable]
    public class GeoLocation
    {
        public readonly string Name;
        public readonly GeoPoint Point;

        public GeoLocation(string name, GeoPoint point)
        {
            Point = point;
            Name = name;
        }
    }

    [Serializable]
    public class GeoLocationSet : SortedSet<GeoLocation>
    {
        public GeoLocationSet()
            :base(new ByLatitude())
        {
        }

        /// <summary>
        /// Find all the locations within a given radius of a specified point
        /// </summary>
        /// <returns>An ordered list of </returns>
        public IEnumerable<KeyValuePair<string,double>> WithinDistance(GeoPoint origin, double distanceInKm)
        {
            const double EarthCircumference = GeoPoint.EarthRadiusKm*2 * Math.PI;


            //scale radiius from km to degrees
            //add 0.5% margin to account for error in distance function
            double distanceInDegrees = distanceInKm/EarthCircumference * 360 * 1.005;

            
            var south = new GeoLocation("", new GeoPoint(Math.Min(90, origin.Latitude - distanceInDegrees), 0));
            var north = new GeoLocation("", new GeoPoint(Math.Min(90,origin.Latitude + distanceInDegrees), 0));
         
            return GetViewBetween(south, north)
                .Select(p => new KeyValuePair<string, double>(p.Name, GeoPoint.DistanceInKm(p.Point, origin)))
                .Where(t => t.Value <= distanceInKm)
                .OrderBy(t => t.Value);
        }
    }

    

    /// <summary>
    /// A point on the surface of the earth represented as Latitude and Longitude
    /// </summary>
    public class GeoPoint
    {
        public const double EarthRadiusKm = 6372.797560856;

        public readonly double Latitude;
        public readonly double Longitude;

        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return String.Format("(Lat:{0}, Lon:{1})", Latitude, Longitude);
        }

        private GeoPoint ToRadians()
        {
            const double r = Math.PI/180;
            return new GeoPoint(Latitude * r, Longitude * r);
        }

        public static double DistanceInKm(GeoPoint a, GeoPoint b)
        {
            return DistanceInRadians(a,b) * EarthRadiusKm;
        }

        public static double DistanceInRadians(GeoPoint a, GeoPoint b)
        {
            a = a.ToRadians();
            b = b.ToRadians();
            double u = Math.Sin((b.Latitude - a.Latitude)/2);
            double v = Math.Sin((b.Longitude - a.Longitude)/2);
            return 2.0*Math.Asin(Math.Sqrt(u*u + Math.Cos(b.Latitude)*Math.Cos(a.Latitude)*v*v));
        }
    }
}
