using System;
using NUnit.Framework;
using OrigoDB.Core.Models;

namespace OrigoDB.Test.NUnit.Models
{
    [TestFixture]
    public class GeoPointTests
    {

        /*
         GEOADD Sicily 13.361389 38.115556 "Palermo" 15.087269 37.502669 "Catania"

(integer) 2

redis>  GEODIST Sicily Palermo Catania

"166274.15156960039"

redis>  GEODIST Sicily Palermo Catania km

"166.27415156960038"

redis>  GEODIST Sicily Palermo Catania mi

"103.31822459492736"

redis>  GEODIST Sicily Foo Bar

(nil)

redis>  
         * */

        private readonly double[][] _distances = 
        {
            new []{13.361389, 38.115556, 15.087269, 37.502669, 166.27415156960038}, 
            new double[]{57.701361, 12.939911, 13.361389, 38.115556, 5}, 
        };

        [Test, TestCaseSource("_distances")]
        public void Distance(double[] data)
        {
            var a = new GeoPoint(data[1], data[0]);
            var b = new GeoPoint(data[3], data[2]);
            var actual = GeoPoint.DistanceInKm(a, b);
            var actualInverse = GeoPoint.DistanceInKm(b, a);
            Assert.AreEqual(actual, actualInverse, "dist(b,a) should equal dist(a,b)");
            Assert.AreEqual(data[4], actual, "0.02");
        }

      
    }
}