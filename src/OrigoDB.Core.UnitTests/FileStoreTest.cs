using System.IO;
using System.Linq;
using System;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
	[TestFixture]
	public class FileStoreTest
	{
	    static ICommandStore _store;
	    EngineConfiguration _config;
	    static string _path = Guid.NewGuid().ToString();

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(_path);
            _config = new EngineConfiguration();
            _config.JournalPath = _path;
            _config.MaxEntriesPerJournalSegment = 10;
            _store = new FileCommandStore(_config);
            _store.Initialize();

            var writer = _store.CreateJournalWriter(0);
            for (ulong i = 0; i < 30; i++)
            {
                writer.Write(new JournalEntry<Command>(i + 1, new TestCommandWithoutResult()));
            }
            writer.Close();
        }
        [TestFixtureSetUp]
        public static void TestFixtureSetup()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }

        //Use TestCleanup to run code after each test has run
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
		

        [Test, ExpectedException(typeof(InvalidOperationException))]
		public void JournalReadThrowsIfSequenceStartIsMissing()
		{
			var files = Directory.GetFiles(_path).OrderBy(f => f).ToArray();
			File.Delete(files[0]);
			_store.Initialize(); // Reload store to update journalfile list.
			_store.GetJournalEntriesFrom(1).Count();
			Assert.Fail();
		}

		[Test]
		public void JournalReadReturnsFirstEntryFirstFile()
		{
			var result = _store.GetJournalEntriesFrom(1).ToArray();
			var id = result[0].Id;
			Assert.AreEqual((ulong) 1,id);
			Assert.AreEqual(30,result.Length);
		}

		[Test]
		public void JournalReadReturnsLastEntryFirstFile()
		{
			var result = _store.GetJournalEntriesFrom(10).ToArray();
			var id = result[0].Id;
			Assert.AreEqual((ulong)10, id);
			Assert.AreEqual(21, result.Length);
		}

		[Test]
		public void JournalReadReturnsMiddleEntryMiddleFile()
		{
			var result = _store.GetJournalEntriesFrom(15).ToArray();
			var id = result[0].Id;
			Assert.AreEqual((ulong)15, id);
			Assert.AreEqual(16, result.Length);
		}
	}
}
