using System;
using OrigoDB.Core.Configuration;

namespace OrigoDB.Core
{
	public class PartitionClusterClientConfiguration : ClientConfiguration
	{
		public static readonly PartitionClusterClientConfiguration Default = new PartitionClusterClientConfiguration();


	    public override IEngine<TModel> GetClient<TModel>()
	    {
	        throw new NotImplementedException();
	    }
	}
}