namespace OrigoDB.Core
{

    public static class Config
    {
        private static Engines _engines = new Engines();

        public static Engines Engines
        {
            get { return _engines; }
        }
    }
}
