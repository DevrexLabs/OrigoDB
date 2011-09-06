using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	internal interface ICommandJournal : IEnumerable<JournalEntry>
	{
		void Open();
		void Append<T>(T item);
		void Close();
		void Clear();
	}
}
