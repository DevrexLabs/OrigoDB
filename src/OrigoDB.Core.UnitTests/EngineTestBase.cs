namespace OrigoDB.Core.Test
{
    public class EngineTestBase
    {

        public EngineConfiguration CreateConfig()        {
            return EngineConfiguration
                .Create().ForIsolatedTest();
        }


    }
}