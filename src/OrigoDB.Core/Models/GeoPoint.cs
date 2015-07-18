using System;

namespace OrigoDB.Core.Models
{

    public class GeoPoint
    {
        public const double EarthRadiusKm = 6372.797560856; //6371;

        public readonly double Latitude;
        public readonly double Longitude;

        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return String.Format("(Lat:{0}, Long:{1})", Latitude, Longitude);
        }

        public string ToGeoHash()
        {
            return "Todo";
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

        public static GeoPoint ParseGeoHash(string geoHash)
        {
            return null;
        }
    }
}
