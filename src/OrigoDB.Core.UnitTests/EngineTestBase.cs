namespace OrigoDB.Core.Test
{
    public class EngineTestBase
    {

        public EngineConfiguration CreateConfig()        {
            return new EngineConfiguration().ForIsolatedTest();
        }


    }
}