using OrigoDB.Core.Proxy;

namespace OrigoDB.Core
{
	public static class Db
	{

        public static T For<T>() where T : Model, new()
        {
            var instance = Engine.For<T>();
            return (T)new ModelProxy<T>(instance).GetTransparentProxy();
        }

		public static T For<T>(string clientIdentifier) where T : Model,new()
		{
			var instance = Engine.For<T>(clientIdentifier);
			return (T)new ModelProxy<T>(instance).GetTransparentProxy();
		}
	}
}