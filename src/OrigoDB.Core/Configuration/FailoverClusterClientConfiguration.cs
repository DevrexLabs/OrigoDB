using System;

namespace OrigoDB.Core
{
	public class FailoverClusterClientConfiguration : ClientConfiguration
	{
		public static readonly FailoverClusterClientConfiguration Default = new FailoverClusterClientConfiguration();


		#region Overrides of ClientConfiguration

		public override IEngine<M> GetClient<M>()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}