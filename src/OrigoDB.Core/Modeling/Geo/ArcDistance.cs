using System;

namespace OrigoDB.Core.Modeling.Geo
{
    /// <summary>
    /// Distance between two points on the surface of the earth
    /// </summary>
    [Serializable]
    public class ArcDistance : IEquatable<ArcDistance>, IComparable<ArcDistance>
    {
        public ArcDistance(double radians)
        {
            Radians = radians;
        }

        public double ToKilometers()
        {
            return Radians * GeoPoint.EarthRadiusKm;
        }

        public double Radians { get; private set; }

        public bool Equals(ArcDistance other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(ArcDistance other)
        {
            return Math.Sign(Radians - other.Radians);
        }

        public override string ToString()
        {
            return ToKilometers() + " km";
        }
    }
}