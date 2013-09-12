using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
	partial class Engine
	{
		public static IEngine<M> For<M>() where M : Model, new()
		{
			return For<M>(typeof(M).Name);
		}

		public static IEngine<M> For<M>(string clientIdentifier) where M : Model, new()
		{
			var config = ClientConfiguration.Create(clientIdentifier);
			return config.GetClient<M>();
		}

		public static IEngine<M> For<M>(EngineConfiguration configuration) where M : Model, new()
		{
			return new LocalClientConfiguration(configuration).GetClient<M>();
		}
	}
}
