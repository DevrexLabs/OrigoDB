using System.Linq;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class SnapshotTests
    {
        [Test]
        public void Snapshots_are_numbered_correctly()
        {
            var config = EngineConfiguration.Create().WithImmutability().ForIsolatedTest();
            var engine = Engine.Create<ImmutableModel>(config);

            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.Execute(new AppendNumberCommand(42));
            engine.CreateSnapshot();

            var store = config.CreateStore();


            Assert.AreEqual(0, store.Snapshots.First().LastEntryId);
            Assert.AreEqual(4, store.Snapshots.Skip(1).First().LastEntryId);
            Assert.AreEqual(2, store.Snapshots.Count());

        }
    }
}