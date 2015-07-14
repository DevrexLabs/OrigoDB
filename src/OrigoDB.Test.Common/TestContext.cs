using System.Dynamic;

namespace OrigoDB.Test.Common
{
    public static class TestContext
    {
        static TestContext ()
        {
            Bag = new ExpandoObject();
        }

        public static dynamic Bag { get; set; }
    }
}
