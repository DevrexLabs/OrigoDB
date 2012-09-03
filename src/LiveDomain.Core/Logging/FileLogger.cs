using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel.Composition;
using LiveDomain.Core.Logging;

namespace LiveDomain.Core
{
    class FileLogger : Logger
    {
        
        StreamWriter _writer;

        public FileLogger(string path)
        {
            _writer = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            
        }

        public FileLogger() : this("log.txt")
        {
            
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
