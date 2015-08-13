using System;

namespace OrigoDB.Core.Test
{

    public static class ConfigurationExtensions
    {
        public static EngineConfiguration WithRandomLocation(this EngineConfiguration config)
        {
            config.JournalPath = Guid.NewGuid().ToString();
            return config;
        }

        public static EngineConfiguration WithInMemoryStore(this EngineConfiguration config)
        {
            config.SetCommandStoreFactory(cfg => new InMemoryCommandStore(cfg));
            config.SetSnapshotStoreFactory(cfg => new InMemorySnapshotStore(cfg));
            return config;
        }

        public static EngineConfiguration ForIsolatedTest(this EngineConfiguration config)
        {
            return config.WithInMemoryStore().WithRandomLocation();
        }
    }
}