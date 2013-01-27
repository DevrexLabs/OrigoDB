using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core.Storage
{
    public class ByteCountingNullStream : Stream
    {
        private long _length;

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
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _length; }
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
            _length += count;
        }
    }
}
