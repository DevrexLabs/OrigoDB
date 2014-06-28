using System;
using System.Collections.Generic;
using System.IO;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
{
    public class FileSnapshotStore : SnapshotStore
    {

        public FileSnapshotStore(EngineConfiguration config)
            :base(config)
        {
        }

        protected override Snapshot WriteSnapshotImpl(Model model)
        {
            var fileSnapshot = new FileSnapshot(DateTime.Now, model.Revision);
            var fileName = Path.Combine(_config.Location.OfSnapshots, fileSnapshot.Name);
            using (Stream stream =  new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                _formatter.Serialize(stream, model);
            }
            return fileSnapshot;
        }

        protected override IEnumerable<Snapshot> ReadSnapshotMetaData()
        {
            var snapshots = new List<FileSnapshot>();
            foreach (var file in Directory.GetFiles(_config.Location.OfSnapshots, "*.snapshot"))
            {
                var fileInfo = new FileInfo(file);
                snapshots.Add(FileSnapshot.FromFileInfo(fileInfo.Name, fileInfo.CreationTime));
            }

            snapshots.Sort((a, b) => a.Revision.CompareTo(b.Revision));
            return snapshots;
        }

        public override Model LoadSnapshot(Snapshot snapshot)
        {
            string snapshotName = ((FileSnapshot)snapshot).Name;
            var directory = _config.Location.OfSnapshots;
            var fileName = Path.Combine(directory, snapshotName);
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return _formatter.Read<Model>(stream);
            }
        }
    }
}