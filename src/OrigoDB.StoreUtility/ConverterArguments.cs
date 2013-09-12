using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace OrigoDB.StoreUtility
{
	public class ConverterArguments : Arguments
	{
		[Option(null, "assembly", Required = true)]
		public string Assembly { get; set; }

		[Option(null, "source", Required = true)]
		public string Source { get; set; }

		[Option(null, "source-type", Required = true)]
		public string SourceType { get; set; }

		[Option(null, "destination", Required = true)]
		public string Destination { get; set; }

		[Option(null, "destination-type", Required = true)]
		public string DestinationType { get; set; }

		[Option(null, "source-snapshots")]
		public string SourceSnapshots { get; set; }

		[Option(null, "destination-snapshots")]
		public string DestinationSnapshots { get; set; }


		public override void Validate()
		{
			if (SourceType == DestinationType)
				throw new ArgumentException("source and destination types must be different");

			if (SourceType == "file-v0.4" && DestinationType == "file" && Source == Destination)
				throw new ArgumentException("source and destination cannot be the same (in-place upgrade not supported at this time)");

			if(string.IsNullOrEmpty(SourceSnapshots))
				SourceSnapshots = Source;
		}
	}
}
