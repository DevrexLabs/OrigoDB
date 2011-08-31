using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	internal class CommandLog : ICommandLog
	{
		private ILogWriter _writer;
		private IStorage _storage;
		bool _isOpen;


		public CommandLog(IStorage storage)
		{
			_storage = storage;
		}

		#region Implementation of IEnumerable

		public IEnumerator<LogItem> GetEnumerator()
		{
			Close();
			var path = _storage.GetLogFilePath();
			var serializer = _storage.CreateSerializer();
			var logStream = _storage.GetReadStream(path);

			using (logStream)
			{
				foreach (var logItem in serializer.ReadToEnd<LogItem>(logStream))
				{
					yield return logItem;
				}
			}
			Open();			
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of ICommandLog

		public void Open()
		{
			throw new NotImplementedException();
			if (_isOpen) return;
			_isOpen = true;
			_writer = _storage.CreateLogWriter();
		}

		public void Write(Command command)
		{
			throw new NotImplementedException();
			var logItem = new LogItem(command);
			_writer.Write(logItem);
		}

		public void Close()
		{
			throw new NotImplementedException();
			if(!_isOpen) return;
			_isOpen = false;
			_writer.Close();
			_writer.Dispose();
		}

		public void Truncate()
		{
			throw new NotImplementedException();
			Close();
			File.Delete(_storage.GetLogFilePath());
			Open();
		}

		#endregion
	}
}
