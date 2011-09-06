using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	internal interface IStorage
	{
		string GetDataFilePath();
		string GetJournalFilePath();
		string GetTempPath();

		Serializer CreateSerializer();
		Stream GetWriteStream(string path,bool append);
		Stream GetReadStream(string path);
		IJournalWriter CreateJournalWriter();
	}
}
