using System;
using NUnit.Framework;
using OrigoDB.Core.Configuration;

namespace OrigoDB.Core.Test
{
    
    
    [TestFixture]
    public class ConfigDictionaryTests
    {

        private ConfigDictionary _configDictionary;
        private EngineConfiguration _expected;

        [SetUp]
        public void Setup()
        {
            _expected = new EngineConfiguration();
            _expected.EnsureSafeResults = false;
            _expected.LockTimeout = TimeSpan.FromSeconds(30);
            _expected.Kernel = Kernels.RoyalFoodTaster;
            _expected.MaxBytesPerJournalSegment = 8192*1024;

            _configDictionary = new ConfigDictionary();
            _configDictionary.Set("kernel", _expected.Kernel);
            _configDictionary.Set("engineconfiguration.ensuresaferesults", _expected.EnsureSafeResults);
            _configDictionary.Set("engineconfiguration.locktimeout", _expected.LockTimeout);
            _configDictionary.Set("EngineConfiguration.MaxbytesPerJournalSegment", _expected.MaxBytesPerJournalSegment);
        }

        [Test]
        public void MapTo_SmokeTest()
        {
            var config = new EngineConfiguration();
            _configDictionary.MapTo(config);
            CompareToExpected(config);
        }

        private void CompareToExpected(EngineConfiguration config)
        {
            Assert.AreEqual(config.Kernel, _expected.Kernel);
            Assert.AreEqual(config.EnsureSafeResults, _expected.EnsureSafeResults);
            Assert.AreEqual(config.MaxBytesPerJournalSegment, _expected.MaxBytesPerJournalSegment);
            Assert.AreEqual(config.LockTimeout, _expected.LockTimeout);
        }


        [Test]
        public void LoadFromString()
        {
            
            string[] validStringsA = {
                                            "a=b",
                                            "a=b;",
                                            " a=b;",
                                            " a=b",
                                            " a=b ",
                                            "a=b ",
                                            "a=b; ",
                                            "a = b",
                                            "a =b",
                                            "a= b "

                                        };
            string[] validStringsB = {
                                            "a=b;b=c",
                                            "a=b;b=c;",
                                            "a=b;b=c; ",
                                            " a=b;b=c;",
                                            " a=b;b=c; ",
                                            " a=b;b=c ",
                                            " a=b;b=c; ",
                                            "a=b;b=c "

                                         };
            foreach(var s in validStringsA)
            {
                Console.WriteLine(s);
                var config = ConfigDictionary.FromDelimitedString(s);
                Assert.AreEqual(1, config.Count);
                Assert.IsTrue(config.ContainsKey("a"));
                Assert.AreEqual("b", config["a"]);
            }

            foreach (var s in validStringsB)
            {
                Console.WriteLine(s);
                var config = ConfigDictionary.FromDelimitedString(s);
                Assert.AreEqual("b", config.Get<string>("a"));
                Assert.AreEqual("c", config.Get<string>("b"));
            }
        }
    }
}
