using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LiveDomain.Core.Configuration;
using LiveDomain.Core.Storage;

namespace LiveDomain.Core
{
    /// <summary>
    /// Storage implementation using a local file system
    /// </summary>
    internal class FileStorage : StorageBase
    {


        //EngineConfiguration EngineConfiguration;


        internal FileStorage(EngineConfiguration config) : base(config)
        {
            //EngineConfiguration = config;
        }

        public override void Initialize()
        {
            EnsureDirectoryExists(EngineConfiguration.Location);

            if (EngineConfiguration.HasAlternativeSnapshotLocation)
            {
                EnsureDirectoryExists(EngineConfiguration.SnapshotLocation); 
            }
        }


        private void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// List all the files in the snapshot and journal directories
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<string> GetItemIdentifiers()
        {
            foreach (string fileName in Directory.GetFiles(EngineConfiguration.Location))
            {
                yield return new FileInfo(fileName).Name;
            }

            if (EngineConfiguration.HasAlternativeSnapshotLocation)
            {
                foreach (string fileName in Directory.GetFiles(EngineConfiguration.SnapshotLocation))
                {
                    yield return new FileInfo(fileName).Name;
                }
            }
        }

        protected override void RemoveSnapshot(string id)
        {
            string path = Path.Combine(EngineConfiguration.SnapshotLocation, id);
            File.Delete(path);
        }

        protected override Stream GetReadStream(string id)
        {
            string path = GetFullyQualifiedPath(id);
            Stream stream = File.OpenRead(path);
            if (EngineConfiguration.Compression == CompressionMethod.GZip)
            {
                stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            return stream;
        }

        protected override bool Exists(string id)
        {
            return File.Exists(GetFullyQualifiedPath(id));
        }

        protected override Stream GetWriteStream(string id, bool append)
        {
            string path = GetFullyQualifiedPath(id);

            var filemode = append ? FileMode.Append : FileMode.Create;
            
            Stream stream = new FileStream(path, filemode, FileAccess.Write);
            if (EngineConfiguration.Compression == CompressionMethod.GZip)
            {
                stream = new GZipStream(stream, CompressionMode.Compress);
            }
            return stream;
        }

        private void VerifyDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                if (!DirectoryEx.IsEmpty(directory))
                {
                    throw new ApplicationException("Directory must be empty: " + directory);
                }
            }
        }

        /// <summary>
        /// Use correct directory based on filetype, journal or snapshot.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetFullyQualifiedPath(string id)
        {
            string directory = EngineConfiguration.Location;
            if (id.EndsWith(StorageBlobIdentifier.SnapshotSuffix))
            {
                directory = EngineConfiguration.SnapshotLocation;
            }
            return Path.Combine(directory,id);
        }

        public override void VerifyCanCreate()
        {
            VerifyDirectory(EngineConfiguration.Location);
            if (EngineConfiguration.HasAlternativeSnapshotLocation)
                VerifyDirectory(EngineConfiguration.SnapshotLocation);
        }

        public override void VerifyCanLoad()
        {
            string error = String.Empty;
            if (!Directory.Exists(EngineConfiguration.Location))
            {
                error = "Target directory does not exist\n";
            }
            else if (Directory.GetFiles(EngineConfiguration.Location, "*.journal").Count() == 0)
            {
                error += "No journal files found in target directory\n";
            }
            

            if (EngineConfiguration.HasAlternativeSnapshotLocation)
            {
                if (!Directory.Exists(EngineConfiguration.SnapshotLocation))
                {
                    error += "Snapshot directory does not exist\n";
                }
            }

            string initialSnapshot = Path.Combine(EngineConfiguration.SnapshotLocation, "000000000.snapshot");
            if (!File.Exists(initialSnapshot))
            {
                error += "Initial snapshot missing\n";
            }



            if (error != String.Empty)
            {
                throw new ApplicationException("Error(s) loading: " + error);   
            }
        }

    }
}
