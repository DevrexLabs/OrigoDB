using System;

namespace OrigoDB.Core
{
	public class PartitionClusterClientConfiguration : ClientConfiguration
	{
		public static readonly PartitionClusterClientConfiguration Default = new PartitionClusterClientConfiguration();


		#region Overrides of ClientConfiguration

		public override IEngine<M> GetClient<M>()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}