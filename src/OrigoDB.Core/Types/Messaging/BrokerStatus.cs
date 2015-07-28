using System;
using System.Collections.Generic;

namespace OrigoDB.Core.Types.Messaging
{
    [Serializable]
    public class BrokerStatus
    {
        public IDictionary<String,int> Queues { get; internal set; }
        public IDictionary<String,IDictionary<Guid,int>> Topics { get; internal set; }

        internal BrokerStatus()
        {
            
        }
    }
}