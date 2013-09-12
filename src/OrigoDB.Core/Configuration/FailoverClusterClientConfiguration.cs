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

		public override IEngine<M> GetClient<M>()
		{
			return new FailoverClusterClient<M>(_baseConfig);
		}

		#endregion
	}
}