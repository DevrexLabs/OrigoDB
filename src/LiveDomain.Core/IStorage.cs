using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	public interface IStorage
	{
		string GetDataFilePath();
		string GetLogFilePath();
		string GetTempPath();

		Serializer CreateSerializer();
		Stream GetWriteStream(string path,bool append);
		Stream GetReadStream(string path);
		ILogWriter CreateLogWriter();
	}
}
