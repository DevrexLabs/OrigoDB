using System;
using System.IO;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Storage
{
    /// <summary>
    /// A stream decorator which keeps track of the number of bytes written
    /// </summary>
    public class ByteCountingStream : Stream
    {
        private long _bytesWritten;
        readonly Stream _stream;

        /// <summary>
        /// Constructor accepting a writeable stream to decorate
        /// </summary>
        /// <param name="stream"></param>
        public ByteCountingStream(Stream stream)
        {
            Ensure.NotNull(stream, "stream");
            Ensure.That(stream.CanWrite, "stream must be writeable");
            _stream = stream;
        }

        /// <summary>
        /// Uses an underlying NullWriteStream
        /// </summary>
        public ByteCountingStream() 
            : this(new NullWriteStream())
        {
            
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Length
        {
            get { return _bytesWritten; }
        }

        public override long Position
        {
            get { return _bytesWritten; }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Ensure.NotNull(buffer, "buffer");
            Ensure.That(offset + count <= buffer.Length, "can't read after end of buffer");
            _stream.Write(buffer,offset,count);
            _bytesWritten += count;
        }
    }

}
