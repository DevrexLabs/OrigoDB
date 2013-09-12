using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    public class FileStorageLocation : StorageLocation
    {

        private string _journalLocation;
        private string _snapshotLocation;
        

        public FileStorageLocation(string location)
        {
            _journalLocation = location;
        }

        /// <summary>
        /// The location of the command journal files. A directory path when using FileStorage, 
        /// a connection string when using SqlStorage.
        /// Assigning a relative path will resolve to current directory or App_Data if running in a web context
        /// </summary>
        public override string OfJournal
        {
            get { return _journalLocation != null ? Path.Combine(GetDefaultDirectory(), _journalLocation) : _journalLocation; }
            set { _journalLocation = value; }
        }

        /// <summary>
        /// Gets the location without combining to an absolute location relative to the default directory
        /// </summary>
        public override string RelativeLocation { get { return _journalLocation; } }

        /// <summary>
        /// Same as Journal unless specifically set
        /// </summary>
        public override string OfSnapshots
        {
            get
            {
                return _snapshotLocation ?? OfJournal;
            }
            set
            {
                if (value == null || value == OfJournal) _snapshotLocation = null;
                else _snapshotLocation = value;
            }
        }


        /// <summary>
        /// True if the snapshotlocation differs from the location of the journal
        /// </summary>
        public override bool HasAlternativeSnapshotLocation
        {
            get
            {
                return _snapshotLocation != null;
            }

        }
    }
}
