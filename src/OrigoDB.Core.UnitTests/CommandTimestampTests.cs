using System;
using System.Linq;
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
                var ts = ExecutionContext.Current.Timestamp;
                model.SetTime(ts);
            }
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

            var before = DateTime.Now;
            ExecutionContext.Begin();
            var command = new SetTimeCommand();
            var target = new JournalAppender(0, fake);
            var after = DateTime.Now;
            target.Append(command);

            Assert.IsNotNull(entry);
            Assert.IsTrue(before <= entry.Created && entry.Created <= after);
        }

        [Test]
        public void Timestamp_preserved_on_restore()
        {
            var command = new SetTimeCommand();
            var config = EngineConfiguration.Create().ForIsolatedTest();
            var engine = Engine.Create<TestModel>(config);
            engine.Execute(command);


            var store = config.CreateCommandStore();
            var entry = store.CommandEntries().Single();
            Assert.AreEqual(entry.Created, ((TestModel) engine.GetModel()).Sometime);
        }
    }
}