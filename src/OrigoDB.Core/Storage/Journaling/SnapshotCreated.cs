namespace OrigoDB.Core.Storage
{

    /// <summary>
    /// Event used for notification
    /// </summary>
    public class SnapshotCreated
    {
        public readonly ulong Revision;

        public SnapshotCreated(ulong revision)
        {
            Revision = revision;
        }
    }
}
