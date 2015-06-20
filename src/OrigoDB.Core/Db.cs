using OrigoDB.Core.Proxy;

namespace OrigoDB.Core
{

    /// <summary>
    /// Helper class for obtaining proxy models
    /// </summary>
	public static class Db
	{

        /// <summary>
        /// Get a proxy for a given type T based on same conventions as Engine.For&lt;T>
        /// </summary>
        public static T For<T>() where T : Model, new()
        {
            var instance = Engine.For<T>();
            return (T)new Proxy<T>(instance).GetTransparentProxy();
        }

        /// <summary>
        /// Get a proxy for a given type T based on same conventions as Engine.For&lt;T>
        /// </summary>
		public static T For<T>(string clientIdentifier) where T : Model,new()
		{
			var instance = Engine.For<T>(clientIdentifier);
			return (T)new Proxy<T>(instance).GetTransparentProxy();
		}

        /// <summary>
        /// Get a proxy for a given type T based on same conventions as Engine.For&lt;T>
        /// </summary>
        public static T For<T>(EngineConfiguration config) where T : Model, new()
        {
            var instance = Engine.For<T>(config);
            return (T)new Proxy<T>(instance).GetTransparentProxy();
            
        }
	}
}