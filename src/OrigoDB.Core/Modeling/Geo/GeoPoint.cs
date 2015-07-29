using System;

namespace OrigoDB.Core.Modeling.Geo
{
    /// <summary>
    /// A point on the surface of the earth
    /// </summary>
    [Serializable]
    public class GeoPoint
    {
        public const double EarthRadiusKm = 6372.797560856;

        /// <summary>
        /// Degrees latitude in the range -90 to +90
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// Degrees Longitude in the range -180 to +180
        /// </summary>
        public double Longitude { get; private set; }

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
            return new GeoPoint(Latitude*r, Longitude*r);
        }

        /// <summary>
        /// Calculate the distance between 2 points along the surface of the earth using the haversine formula
        /// </summary>
        public static ArcDistance Distance(GeoPoint a, GeoPoint b)
        {
            a = a.ToRadians();
            b = b.ToRadians();
            double u = Math.Sin((b.Latitude - a.Latitude) / 2);
            double v = Math.Sin((b.Longitude - a.Longitude) / 2);
            var radians = 2.0 * Math.Asin(Math.Sqrt(u * u + Math.Cos(b.Latitude) * Math.Cos(a.Latitude) * v * v));
            return new ArcDistance(radians);
        }

        /// <summary>
        /// The distance to a point, see Distance
        /// </summary>
        /// <param name="other"></param>
        public ArcDistance DistanceTo(GeoPoint other)
        {
            return Distance(this, other);
        }
    }
}