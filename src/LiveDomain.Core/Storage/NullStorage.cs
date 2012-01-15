using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    /// <summary>
    /// Store nothing storage. Intended for use while running automated functional tests.
    /// </summary>
    internal class NullStorage : IStorage
    {
        Model _model;

        public IEnumerable<JournalEntry<Command>> GetJournalEntries(JournalSegmentInfo position)
        {
            //Empty array as opposed to null
            return new JournalEntry<Command>[0];
        }

        public void VerifyCanLoad()
        {
            
        }

        public Model GetMostRecentSnapshot(out JournalSegmentInfo journalState)
        {
            journalState = JournalSegmentInfo.Initial;
            return _model;
        }

        public void WriteSnapshot(Model model, string name)
        {
            
        }

        public System.IO.Stream CreateJournalWriterStream(JournalWriterCreateOptions options)
        {
            return new NullWriterStream();
        }

        public void VerifyCanCreate()
        {
            
        }

        public void Create(Model model)
        {
            _model = model;
        }

        public bool Exists
        {
            get { return false; }
        }

        public bool CanCreate
        {
            get { return true; }
        }
    }
}
