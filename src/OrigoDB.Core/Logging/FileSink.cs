using System.IO;

namespace OrigoDB.Core.Logging
{
    internal class FileSink : LogSink
    {
        
        readonly StreamWriter _writer;

        public FileSink(string path)
        {
			_writer = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
        }

        public FileSink() : this("log.txt")
        {
        }

        ~FileSink()
        {
            if(_writer != null) _writer.Dispose();
        }

        public override void WriteMessage(string message)
        {
            _writer.Write(message);
            _writer.Flush();
        }

    }
}
