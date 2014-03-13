using System;

namespace OrigoDB.Core
{
	public class FailoverClusterClientConfiguration : ClientConfiguration
	{
		readonly RemoteClientConfiguration _baseConfig;

		public FailoverClusterClientConfiguration(RemoteClientConfiguration baseConfig)
		{
			_baseConfig = baseConfig;
		}

		#region Overrides of ClientConfiguration

		public override IEngine<TModel> GetClient<TModel>()
		{
			return new FailoverClusterClient<TModel>(_baseConfig);
		}

		#endregion
	}
}