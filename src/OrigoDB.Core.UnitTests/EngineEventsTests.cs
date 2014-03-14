using System;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class EngineEventsTests
    {
            
        [Test]
        public void CommandExecuting_is_fired()
        {
            var config = EngineConfiguration.Create().WithImmutability();
            config.SetStoreFactory( cfg => new InMemoryStore(cfg));
            config.Location.OfJournal = Guid.NewGuid().ToString();
            var engine = Engine.Create<ImmutableModel>(config);
            
            bool wasFired = false;
            engine.BeforeExecute += (s, e) =>
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
            var config = EngineConfiguration.Create().WithImmutability();
            config.SetStoreFactory(cfg => new InMemoryStore(cfg));
            config.Location.OfJournal = Guid.NewGuid().ToString();
            var engine = Engine.Create<ImmutableModel>(config);

            engine.BeforeExecute += (s, e) =>
            {
                e.Cancel = true;
            };
            engine.Execute(new AppendNumberCommand(42));
            Config.Engines.CloseAll();
        }
    }
}