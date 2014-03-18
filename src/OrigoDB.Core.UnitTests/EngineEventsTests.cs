using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class EngineEventsTests
    {

        [Test]
        public void Commands_executed_event_contains_sequential_entry_ids()
        {
            var config = EngineConfiguration.Create()
                .ForImmutability(
                ).ForIsolatedTest();
            var engine = Engine.Create<ImmutableModel>(config);
            
            var sequence = new List<ulong>();
            engine.CommandExecuted += (s, e) => sequence.Add(e.JournalEntryId);

            for(int i = 1; i <=100; i++) engine.Execute(new AppendNumberCommand(i));
            foreach (var entryId in sequence)
            {
                Console.WriteLine(entryId);
            }
            var sum = engine.Execute((ImmutableModel m) => m.Numbers().Sum());
            Assert.AreEqual((decimal) sum, sequence.Sum<ulong>(n => (decimal) n));


        }
            
        [Test]
        public void CommandExecuting_is_fired()
        {
            var config = EngineConfiguration.Create().ForImmutability();
            config.SetStoreFactory( cfg => new InMemoryStore(cfg));
            config.Location.OfJournal = Guid.NewGuid().ToString();
            var engine = Engine.Create<ImmutableModel>(config);
            
            bool wasFired = false;
            engine.CommandExecuting += (s, e) =>
            {
                wasFired = true;
            };
            engine.Execute(new AppendNumberCommand(42));
            Assert.AreEqual(true, wasFired);
            Config.Engines.CloseAll();
        }

        [Test, ExpectedException(typeof(CommandAbortedException))]
        public void CommandExecuting_can_cancel()
        {
            var config = EngineConfiguration.Create().ForImmutability();
            config.SetStoreFactory(cfg => new InMemoryStore(cfg));
            config.Location.OfJournal = Guid.NewGuid().ToString();
            var engine = Engine.Create<ImmutableModel>(config);

            engine.CommandExecuting += (s, e) =>
            {
                e.Cancel = true;
            };
            engine.Execute(new AppendNumberCommand(42));
            Config.Engines.CloseAll();
        }
    }
}