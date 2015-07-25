using System.IO;
using System.Linq;
using System;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for FileStoreTest and is intended
    ///to contain all FileStoreTest Unit Tests
    ///</summary>
	[TestFixture]
	public class FileStoreTest
	{
	    static ICommandStore _store;
	    EngineConfiguration _config;
	    static string _path = Guid.NewGuid().ToString();

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
