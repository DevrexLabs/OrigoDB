using System.Text.RegularExpressions;

namespace OrigoDB.Core
{
	partial class Engine
	{
		public static IEngine<TModel> For<TModel>() where TModel : Model, new()
		{
			//return For<TModel>(typeof(TModel).Name);
            var type = typeof(TModel);
            var arguments = type.GetGenericArguments();
            var name = Regex.Replace(type.Name, @"`\d*", "_" + arguments[0].Name);
            return For<TModel>(name);
		}

		public static IEngine<TModel> For<TModel>(string clientIdentifier) where TModel : Model, new()
		{
			var config = ClientConfiguration.Create(clientIdentifier);
			return config.GetClient<TModel>();
		}

		public static IEngine<TModel> For<TModel>(EngineConfiguration configuration) where TModel : Model, new()
		{
			return new LocalClientConfiguration(configuration).GetClient<TModel>();
		}
	}
}
