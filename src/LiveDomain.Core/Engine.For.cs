using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	partial class Engine
	{
		public static IEngine<M> For<M>() where M : Model, new()
		{
			return For<M>(typeof(M).Name);
		}

		public static IEngine<M> For<M>(string clientIdentifier) where M : Model, new()
		{
			//Could return an EnterpriseClientConfiguration
			var config = ClientConfiguration.Create(clientIdentifier);
			return config.GetClient<M>();
		}
	}
}
