using System;
using System.Collections.Generic;

namespace OrigoDB.Core.Models
{
    [Serializable]
    public class ByLatitude : IComparer<GeoPoint>
    {
        public int Compare(GeoLocation x, GeoLocation y)
        {
            int result = Math.Sign(x.Point.Latitude - y.Point.Latitude);
            if (result == 0) result = String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            return result;
        }
    }
}