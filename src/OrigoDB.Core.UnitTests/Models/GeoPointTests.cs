using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core.Models;
using OrigoDB.Core.Types;

namespace OrigoDB.Test.NUnit.Models
{
    [TestFixture]
    public class GeoPointTests
    {
        private readonly double[][] _distances =
        {
            //palermo, catania
            new[] {13.361389, 38.115556, 15.087269, 37.502669, 166.27415156960038},
        };

        [Test, TestCaseSource("_distances")]
        public void Distance(double[] data)
        {
            var a = new GeoPoint(data[1], data[0]);
            var b = new GeoPoint(data[3], data[2]);
            var actual = GeoPoint.DistanceInKm(a, b);
            var actualInverse = GeoPoint.DistanceInKm(b, a);
            Assert.AreEqual(actual, actualInverse, "dist(b,a) should equal dist(a,b)");
            double faultTolerance = data[4]*0.005;
            Assert.AreEqual(data[4], actual, faultTolerance);
        }


        public IEnumerable<GeoPoint> TestPoints()
        {
            for (int lon = -180; lon <= 180; lon += 10)
            {
                for (int lat = -80; lat < 90; lat += 10)
                {
                    yield return new GeoPoint(lat, lon);
                }
            }
        }

        [Test]
        public void ViewBetweenTest()
        {
            var lower = new GeoLocation("lower", new GeoPoint(0, 20));
            var upper = new GeoLocation("upper", new GeoPoint(0, 40));
            var range = new Range<double>(20, 40);
            var between = _set.GetViewBetween(lower, upper);
            foreach (var geoLocation in between)
            {
                Assert.IsTrue(range.Contains(geoLocation.Point.Longitude));
            }
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            _set = new GeoLocationSet();

            for (int lat = -80; lat < 80; lat += 2)
            {
                for (int lon = -179; lon < 180; lon += 2)
                {
                    var name = lat + "_" + lon;
                    _set.Add(new GeoLocation(name, new GeoPoint(lat, lon)));
                }
            }    
        }

        private GeoLocationSet _set;

        [Test, TestCaseSource("TestPoints")]
        public void WithinRadiusTests(GeoPoint search)
        {
            const double radius = 100;


            var within = _set.WithinDistance(search, radius).ToArray();

            foreach (var keyValuePair in within)
            {
                Assert.IsTrue(keyValuePair.Value <= radius);
                Console.WriteLine("Key: " + keyValuePair.Key);
            }

            var withinNames = new HashSet<string>(within.Select(kvp => kvp.Key));
            foreach (var location in _set)
            {
                var name = location.Name;
                var distance = GeoPoint.DistanceInKm(search, location.Point);
                Assert.IsTrue(distance > radius ^ withinNames.Contains(name));
            }
        }
    }
}