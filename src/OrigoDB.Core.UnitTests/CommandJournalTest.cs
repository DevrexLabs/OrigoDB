using System.Linq;
using System;
using System.Collections.Generic;
using OrigoDB.Core.Journaling;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{


    /// <summary>
    ///This is a test class for CommandJournalTest and is intended
    ///to contain all CommandJournalTest Unit Tests
    ///</summary>
    [TestFixture]
    public class CommandJournalTest
    {

        //public class NullStorex: IStore
        //{
        //    private MemoryJournal _memoryJournal;
        //    public NullStorex(MemoryJournal journal = null)
        //    {
        //        _memoryJournal = journal;
        //    }
        //    public void VerifyCanLoad()
        //    {

        //    }

        //    public Model LoadMostRecentSnapshot(out long lastEntryId)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void WriteSnapshot(Model model)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void VerifyCanCreate()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Create(Model model)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Load()
        //    {

        //    }

        //    public bool Exists
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    public IEnumerable<JournalEntry> GetJournalEntries()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public IEnumerable<JournalEntry> GetJournalEntriesFrom(long entryId)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public IEnumerable<JournalEntry> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public IJournalWriter CreateJournalWriter(long lastEntryId)
        //    {
        //        return _memoryJournal ?? new MemoryJournal();
        //    }


        //    public System.IO.Stream CreateJournalWriterStream(long firstEntryId = 1)
        //    {
        //        throw new NotImplementedException();
        //    }


        //    public Model LoadModel()
        //    {
        //        throw new NotImplementedException();
        //    }


        //    public CommandJournal Journal
        //    {
        //        get { throw new NotImplementedException(); }
        //    }


        //    public int LastEntryId
        //    {
        //        get { throw new NotImplementedException(); }
        //    }

        //    public void AppendCommand(Command command)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void InvalidatePreviousCommand()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public class MemoryJournal : IJournalWriter
        //{
        //    private bool isDisposed;
        //    public List<JournalEntry> Entries { get; private set; }


        //    public MemoryJournal()
        //    {
        //        Entries = new List<JournalEntry>();
        //    }

        //    public void Write(JournalEntry item)
        //    {
        //        EnsureNotDisposed();
        //        Entries.Add(item);
        //    }

        //    public void Close()
        //    {
        //        EnsureNotDisposed();
        //        Dispose();
        //    }

        //    public void Dispose()
        //    {
        //        isDisposed = true;
        //    }

        //    private void EnsureNotDisposed()
        //    {
        //        if (isDisposed) throw new ObjectDisposedException("MemoryJournal");
        //    }
        //}


        List<Tuple<List<JournalEntry>, string>> _testCases = new List<Tuple<List<JournalEntry>, string>>()
                                {
                                    Tuple.Create(GenerateEntries(5, 3), "Intermediate entry rolled back"),
                                    Tuple.Create(GenerateEntries(5, 1), "First entry rolled back"),
                                    Tuple.Create(GenerateEntries(5, 5), "Last entry rolled back"),
                                    Tuple.Create(GenerateEntries(10, 9), "Next last entry rolled back"),
                                    Tuple.Create(GenerateEntries(10, 2), "Second entry rolled back"),
                                    Tuple.Create(GenerateEntries(10, 1, 2), "Two first entries rolled back"),
                                    Tuple.Create(GenerateEntries(10, 4, 5), "Two consecutive entries rolled back"),
                                    Tuple.Create(GenerateEntries(10, 9, 10), "Two last entries rolled back"),
                                    Tuple.Create(GenerateEntries(100, 9, 10, 11, 48, 62, 63), "Mixed patches of single/multiple entries rolled back"),
                                    Tuple.Create(GenerateEntries(13, 1, 13), "First and last entries rolled back"),
                                };

        [TestCaseSource("_testCases")]
        public void RolledBackCommandsAreSkipped(Tuple<List<JournalEntry>, string> testCase)
        {
            var target = new CommandJournal(new InMemoryStore());

            string failureMessage = testCase.Item2;
            var testEntries = testCase.Item1;

            //Act
            var actualCommandEntries = target.CommittedCommandEntries(() => testEntries).ToArray();

            long[] rolledBackIds = testEntries.OfType<JournalEntry<RollbackMarker>>().Select(e => e.Id).ToArray();
            int expectedNumberOfCommandEntries = testEntries.Count - rolledBackIds.Length * 2;

            //Assert
            Assert.AreEqual(expectedNumberOfCommandEntries, actualCommandEntries.Length, failureMessage);

            int sequentialId = 1;

            foreach (JournalEntry<Command> journalEntry in actualCommandEntries)
            {
                if (!rolledBackIds.Contains(sequentialId))
                {
                    //TODO: Maybe we should return no-op commands instead of skipping rolled back
                    int expectedId = sequentialId + rolledBackIds.Count(id => id < journalEntry.Id);
                    Assert.AreEqual(expectedId, journalEntry.Id, failureMessage);
                }
                sequentialId++;
            }
        }

        private static List<JournalEntry> GenerateEntries(int numEntries, params int[] failedCommandIds)
        {
            var testEntries = new List<JournalEntry>();
            for (int i = 1; i <= numEntries; i++)
            {
                testEntries.Add(new JournalEntry<Command>(i, null));
                if (failedCommandIds.Contains(i)) testEntries.Add(new JournalEntry<RollbackMarker>(i, null));
            }
            return testEntries;
        }


        [Test]
        public void RollbackMarkerIsWrittenOnRollback()
        {
            //Arrange
            var store = new InMemoryStore();
            var target = new CommandJournal(store);
            target.Append(new ACommand());
            target.Append(new ACommand());

            //Act
            target.WriteRollbackMarker();

            //Assert
            Assert.AreEqual(3, store.GetJournalEntries().Count());
            Assert.AreEqual(1, store.GetJournalEntries().OfType<JournalEntry<RollbackMarker>>().Count());
            Assert.IsTrue(store.GetJournalEntries().Last() is JournalEntry<RollbackMarker>);
        }

        [Serializable]
        private class ACommand : Command
        {
            internal override void PrepareStub(Model model)
            {
                throw new NotImplementedException();
            }

            internal override object ExecuteStub(Model model)
            {
                throw new NotImplementedException();
            }
        }
    }
}
