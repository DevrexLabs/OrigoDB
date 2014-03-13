using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class EngineEventsTests
    {
        [Test]
        public void CommandExecuting_is_fired()
        {
            var store = new InMemoryStore();
            var config = EngineConfiguration.Create().WithImmutability();
            config.SetStoreFactory( cfg => store);
            var engine = Engine.Create<ImmutableModel>(config);
            
            bool wasFired = false;
            engine.BeforeExecute += (s, e) =>
            {
                wasFired = true;
            };
            engine.Execute(new AppendNumberCommand(42));
            Assert.AreEqual(true, wasFired);
        }

        [Test, ExpectedException(typeof(CommandAbortedException))]
        public void CommandExecuting_can_cancel()
        {
            var store = new InMemoryStore();
            var config = EngineConfiguration.Create();
            config.SetStoreFactory(cfg => store);
            var engine = Engine.Create<ImmutableModel>(config);

            engine.BeforeExecute += (s, e) =>
            {
                e.Cancel = true;
            };
            engine.Execute(new AppendNumberCommand(42));
        }
    }
}