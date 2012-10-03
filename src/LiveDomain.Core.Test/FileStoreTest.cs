using System.IO;
using System.Linq;
using LiveDomain.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace LiveDomain.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for FileStoreTest and is intended
    ///to contain all FileStoreTest Unit Tests
    ///</summary>
	[TestClass()]
	public class FileStoreTest
	{


		private TestContext testContextInstance;
	    static IStore _store;
	    EngineConfiguration _config;
	    static string _livedbFilestoretest = "C:/livedb/filestoreTest";

	    /// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			if(Directory.Exists(_livedbFilestoretest)) Directory.Delete(_livedbFilestoretest, true);
			Directory.CreateDirectory(_livedbFilestoretest);
		}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		[TestInitialize()]
		public void MyTestInitialize()
		{
			if (!Directory.Exists(_livedbFilestoretest)) Directory.CreateDirectory(_livedbFilestoretest);
			_config = EngineConfiguration.Create();
			_config.Location = _livedbFilestoretest;
			_config.MaxEntriesPerJournalSegment = 10;
			_store = new FileStore(_config);
			_store.Load();

			var writer = _store.CreateJournalWriter(0);
			for (int i = 0; i < 30; i++)
			{
				writer.Write(new JournalEntry<Command>(i + 1, new TestCommandWithoutResult()));
			}
			writer.Close();
		}
		//
		//Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup()
		{
			Directory.Delete(_livedbFilestoretest, true);
		}
		
		#endregion

		[TestMethod(), ExpectedException(typeof(FileNotFoundException))]
		public void JournalReadThrowsIfSequenceStartIsMissing()
		{
			var files = Directory.GetFiles(_livedbFilestoretest).OrderBy(f => f).ToArray();
			File.Delete(files[0]);
			Assert.AreEqual(30, _store.GetJournalEntriesFrom(1).Count());
		}

		[TestMethod()]
		public void JournalReadReturnsFirstEntryFirstFile()
		{
			var result = _store.GetJournalEntriesFrom(1).ToArray();
			var id = result[0].Id;
			Assert.AreEqual(1,id);
			Assert.AreEqual(30,result.Length);
		}

		[TestMethod()]
		public void JournalReadReturnsLastEntryFirstFile()
		{
			var result = _store.GetJournalEntriesFrom(10).ToArray();
			var id = result[0].Id;
			Assert.AreEqual(10, id);
			Assert.AreEqual(21, result.Length);
		}

		[TestMethod()]
		public void JournalReadReturnsMiddleEntryMiddleFile()
		{
			var result = _store.GetJournalEntriesFrom(15).ToArray();
			var id = result[0].Id;
			Assert.AreEqual(15, id);
			Assert.AreEqual(16, result.Length);
		}
	}
}
