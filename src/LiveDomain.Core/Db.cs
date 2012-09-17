using LiveDomain.Core.Proxy;

namespace LiveDomain.Core
{
	public static class Db
	{
		public static T For<T>(string clientIdentifier) where T : Model,new()
		{
			var instance = Engine.For<T>(clientIdentifier);
			return (T)new ModelProxy<T>(instance).GetTransparentProxy();
		}
	}
}