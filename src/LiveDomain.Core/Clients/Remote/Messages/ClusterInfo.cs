using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	[Serializable]
	public class ClusterInfo
	{
		public Guid Id { get; set; }

		public string MasterHost { get; set; }
		public int MasterPort { get; set; }

		// Slaves with Host, Port
		public Dictionary<string, int> Slaves { get; private set; }
	}

	[Serializable]
	public class ClusterInfoRequest : NetworkMessage
	{
	}

	[Serializable]
	public class ClusterInfoResponse : NetworkMessage
	{
		public ClusterInfo ClusterInfo { get; set; }		
	}

	[Serializable]
	public class ClusterExecuteRequest : NetworkMessage
	{
		public ClusterExecuteRequest(Guid clusterId,object objectToExecute)
		{
			ClusterId = clusterId;
			ObjectToExecute = objectToExecute;
		}

		public Guid ClusterId { get; set; }
		public object ObjectToExecute { get { return Payload; } set { Payload = value; } }
	}
	
	/// <summary>
	/// Execution response from server with the result as payload.
	/// Also notifies clients if there is a change in the Cluster.
	/// </summary>
	[Serializable]
	public class ClusterExecuteResponse : NetworkMessage
	{
		public bool ExecutionCompleted { get; set; }	
		public bool ClusterUpdated { get { return ClusterInfo != null; } }
		public ClusterInfo ClusterInfo { get; set; }

	}
}
