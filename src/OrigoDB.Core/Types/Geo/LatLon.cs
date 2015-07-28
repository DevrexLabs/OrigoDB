using System;

namespace OrigoDB.Core.Types.Geo
{
    /// <summary>
    /// A point on the surface of the earth
    /// </summary>
    [Serializable]
    public class LatLon
    {
        public const double EarthRadiusKm = 6372.797560856;

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public LatLon(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return String.Format("(Lat:{0}, Lon:{1})", Latitude, Longitude);
        }

        private LatLon ToRadians()
        {
            const double r = Math.PI/180;
            return new LatLon(Latitude*r, Longitude*r);
        }

        public static ArcDistance Distance(LatLon a, LatLon b)
        {
            return DistanceInRadians(a, b);
        }

        public ArcDistance DistanceTo(LatLon other)
        {
            return Distance(this, other);
        }

        public static ArcDistance DistanceInRadians(LatLon a, LatLon b)
        {
            a = a.ToRadians();
            b = b.ToRadians();
            double u = Math.Sin((b.Latitude - a.Latitude)/2);
            double v = Math.Sin((b.Longitude - a.Longitude)/2);
            var radians = 2.0*Math.Asin(Math.Sqrt(u*u + Math.Cos(b.Latitude)*Math.Cos(a.Latitude)*v*v));
            return new ArcDistance(radians);
        }
    }
}