using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OrigoDB.Core.Storage
{
    /// <summary>
    /// Base class for concrete Snapshot store implementations
    /// </summary>
    public abstract class SnapshotStore : Initializable, ISnapshotStore
    {

        protected IFormatter _formatter;
        protected List<Snapshot> _snapshots;
        protected readonly EngineConfiguration _config;

        public abstract Model LoadSnapshot(Snapshot snapshot);
        protected abstract IEnumerable<Snapshot> ReadSnapshotMetaData();
        protected abstract Snapshot WriteSnapshotImpl(Model model);

        public virtual IEnumerable<Snapshot> Snapshots
        {
            get
            {
                return _snapshots;
            }
        }

        protected SnapshotStore(EngineConfiguration config)
        {
            _config = config;
            _formatter = config.CreateFormatter(FormatterUsage.Snapshot);
        }

        public override void Initialize()
        {
            _snapshots = new List<Snapshot>();
            foreach (var snapshot in ReadSnapshotMetaData())
            {
                _snapshots.Add(snapshot);
            }
            base.Initialize();
        }

        public void WriteSnapshot(Model model)
        {
            EnsureInitialized();
            Snapshot snapshot = WriteSnapshotImpl(model);
            _snapshots.Add(snapshot);
        }



        public bool IsEmpty
        {
            get
            {
                EnsureInitialized();
                return !_snapshots.Any();
            }
        }
    }
}