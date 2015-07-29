using System;
using OrigoDB.Core.Modeling.Geo;

namespace OrigoDB.Core.Modeling.Redis
{
    [Serializable]
    public class NamedGeoPoint
    {
        public string Name { get; private set; }
        public GeoPoint Point { get; private set; }
        public NamedGeoPoint(string name, GeoPoint point)
        {
            Name = name;
            Point = point;
        }
    }
}