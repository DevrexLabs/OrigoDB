using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	[Serializable]
	public class ServerResponse
	{
		public object Result { get; set; }
		public Exception Error { get; set; }
		public bool Succeeded { get { return Error == null; } }	
	}
}
