using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LiveDomain.Core
{
    class FileLogger : Logger
    {
        
        StreamWriter _writer;

        public FileLogger(string path)
        {
            _writer = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write));
            
        }

        protected override void WriteToLog(string message)
        {
            _writer.Write(message);
        }

        public override void Dispose()
        {
            _writer.Close();
        }
    }
}
