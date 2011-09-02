using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	[Serializable]
	public class LogItem
	{
        internal ILogCommand _command;

        internal readonly DateTime Created;
		
		internal LogItem(ILogCommand command)
		{
            _command = command;
			Created = DateTime.Now;
		}
	}
}
