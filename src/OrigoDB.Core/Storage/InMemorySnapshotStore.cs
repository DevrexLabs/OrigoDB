using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core.Test
{
    public class InMemorySnapshotStore : SnapshotStore
    {
        

        static readonly Dictionary<string, InMemorySnapshotStoreState> _states
            = new Dictionary<string, InMemorySnapshotStoreState>();

        readonly InMemorySnapshotStoreState _state;


        public InMemorySnapshotStore(EngineConfiguration config)
            : base(config)
        {
            string key = _config.Location.OfSnapshots;
            if (!_states.ContainsKey(key)) _states.Add(key, new InMemorySnapshotStoreState());
            _state = _states[key];
        }

        private class InMemorySnapshotStoreState
        {
            public readonly Dictionary<Snapshot, byte[]> Snapshots;

            public InMemorySnapshotStoreState()
            {
                Snapshots = new Dictionary<Snapshot, byte[]>();
            }
        }

        protected override Snapshot WriteSnapshotImpl(Model model)
        {
            var bytes = _formatter.ToByteArray(model);
            var snapshot = new Snapshot(DateTime.Now, model.Revision);
            _state.Snapshots.Add(snapshot, bytes);
            return snapshot;
        }


        public override Model LoadSnapshot(Snapshot snapshot)
        {
            if (!_state.Snapshots.ContainsKey(snapshot))
            {
                throw new ArgumentException("No such snapshot");
            }

            return _formatter.FromByteArray<Model>(_state.Snapshots[snapshot]);
        }



        protected override IEnumerable<Snapshot> ReadSnapshotMetaData()
        {
            return _state.Snapshots.Keys.OrderBy(ss => ss.Revision);
        }
    }
}