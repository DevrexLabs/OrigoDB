using System;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class CommandTimestampTests
    {

        [Serializable]
        class TestModel : Model
        {
            public DateTime Sometime;

            public void SetTime(DateTime time)
            {
                Sometime = time;
            }
        }

        [Serializable]
        class SetTimeCommand : Command<TestModel>
        {

            public override void Execute(TestModel model)
            {
                model.SetTime(Timestamp);
            }
        }


        [Test]
        public void Read_timestamp_throws_when_unassigned()
        {
            var command = new SetTimeCommand();
            Assert.Catch(() => { var _ = command.Timestamp; });
        }

        [Test]
        public void Timestamp_is_copied_to_journal_entry()
        {
            JournalEntry entry = null;

            var fake = A.Fake<IJournalWriter>();
            
            A.CallTo(() => fake.Write(A<JournalEntry>._))
                .Invokes((JournalEntry je) =>
                {
                    entry = je;
                });
            
            var command = new SetTimeCommand();
            command.Timestamp = DateTime.Now;
            var target = new JournalAppender(0, fake);
            target.Append(command);

            Assert.IsNotNull(entry);
            Assert.AreEqual(command.Timestamp, entry.Created);
        }

        [Test]
        public void Timestamp_preserved_on_restore()
        {
            var command = new SetTimeCommand();
            var config = EngineConfiguration.Create().ForIsolatedTest();
            var engine = Engine.Create<TestModel>(config);
            engine.Execute(command);


            var store = config.CreateStore();
            var entry = store.CommandEntries().Single();
            Assert.AreEqual(entry.Created, entry.Item.Timestamp);
            Assert.AreEqual(command.Timestamp, entry.Created);
        }
    }
}