using System;

namespace LiveDomain.Core
{
	[Serializable]
	public class SnapshotRequest : NetworkMessage<SnapshotResponse>
	{
	}

	[Serializable]
	public class SnapshotResponse : NetworkMessage
	{
		public byte[] Snapshot { get { return (byte[]) Payload; } }
	}
}