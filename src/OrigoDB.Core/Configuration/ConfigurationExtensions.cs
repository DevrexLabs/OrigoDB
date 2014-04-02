using System;

namespace OrigoDB.Core.Test
{
    /// <summary>
    /// Some extensions to aid setting up for test
    /// </summary>
    public static class ConfigurationExtensions
    {
        public static EngineConfiguration WithRandomLocation(this EngineConfiguration config)
        {
            config.Location.OfJournal = Guid.NewGuid().ToString();
            return config;
        }

        public static EngineConfiguration WithInMemoryStore(this EngineConfiguration config)
        {
            config.SetStoreFactory(cfg => new InMemoryStore(cfg));
            return config;
        }

        public static EngineConfiguration ForIsolatedTest(this EngineConfiguration config)
        {
            return config.WithInMemoryStore().WithRandomLocation();
        }
    }
}