using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	internal class CommandJournal : ICommandJournal
	{
		private IJournalWriter _writer;
		private IStorage _storage;
		bool _isOpen;


		public CommandJournal(IStorage storage)
		{
			_storage = storage;
		}

		#region Implementation of IEnumerable

		public IEnumerator<JournalEntry> GetEnumerator()
		{
			Close();
			var path = _storage.GetJournalFilePath();
            if (File.Exists(path))
            {
                var serializer = _storage.CreateSerializer();
                var stream = _storage.GetReadStream(path);

                using (stream)
                {
                    foreach (var journalEntry in serializer.ReadToEnd<JournalEntry>(stream))
                    {
                        yield return journalEntry;
                    }
                }
            }
			Open();			
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of ICommandJournal

		public void Open()
		{
			if (_isOpen) return;
			_isOpen = true;
			_writer = _storage.CreateJournalWriter();
		}

		public void Append<T>(T command)
		{
            if (typeof(T) != typeof(Command)) throw new ArgumentException("Argument must be of type T","command");
			var entry = new JournalEntry<Command>(command as Command);
			_writer.Write(entry);
		}

		public void Close()
		{
			if(!_isOpen) return;
			_isOpen = false;
			_writer.Close();
			_writer.Dispose();
		}

		public void Clear()
		{
			Close();
			File.Delete(_storage.GetJournalFilePath());
			Open();
		}

		#endregion
	}
}
