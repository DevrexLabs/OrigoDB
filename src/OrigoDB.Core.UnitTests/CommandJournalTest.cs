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

            ulong[] rolledBackIds = testEntries.OfType<JournalEntry<RollbackMarker>>().Select(e => e.Id).ToArray();
            int expectedNumberOfCommandEntries = testEntries.Count - rolledBackIds.Length * 2;

            //Assert
            Assert.AreEqual(expectedNumberOfCommandEntries, actualCommandEntries.Length, failureMessage);

            ulong sequentialId = 1;

            foreach (JournalEntry<Command> journalEntry in actualCommandEntries)
            {
                if (!rolledBackIds.Contains(sequentialId))
                {
                    //TODO: Maybe we should return no-op commands instead of skipping rolled back
                    ulong expectedId = sequentialId + (ulong) rolledBackIds.Count(id => id < journalEntry.Id);
                    Assert.AreEqual(expectedId, journalEntry.Id, failureMessage);
                }
                sequentialId++;
            }
        }

        private static List<JournalEntry> GenerateEntries(ulong numEntries, params ulong[] failedCommandIds)
        {
            var testEntries = new List<JournalEntry>();
            for (ulong i = 1; i <= numEntries; i++)
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
