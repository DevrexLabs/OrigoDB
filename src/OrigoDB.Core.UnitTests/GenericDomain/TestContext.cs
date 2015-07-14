using System.Dynamic;

namespace OrigoDB.Test.NUnit.GenericDomain
{
    public static class TestContext
    {
        static TestContext()
        {
            Bag = new ExpandoObject();
        }

        public static dynamic Bag { get; set; }
    }
}
