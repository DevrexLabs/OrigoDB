using System;
using System.IO;
using System.Security.Cryptography;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
    [Flags]
    public enum PacketOptions : byte
    {
        None = 0,
        Encryted = 1,
        Compressed = 2,
        Checksum = 4,
        All = Encryted | Compressed | Checksum
    }

    /// <summary>
    /// Container for bytes to be written to the journal, takes options when
    /// <remarks>
    /// Header format:
    /// 1. Byte   : PacketOptions
    /// 2. Int32  : Number of payload bytes
    /// 3. Byte[] : Payload bytes
    /// 4. Int16  : Number of Checksum bytes. Optional, only when using Checksums
    /// 5. Byte[] Checksum bytes. Optional, only when using Checksums
    /// </remarks>
    /// </summary>
    public class Packet
    {

        [ThreadStatic] private static HashAlgorithm _hasher;

        readonly PacketOptions _options;
        byte[] _checksum;
        byte[] _bytes;
        
        private static HashAlgorithm Hasher
        {
            get
            {
                if (_hasher == null) _hasher = new MD5CryptoServiceProvider();
                return _hasher;
            }
        }

        public static Packet Create(byte[] bytes, PacketOptions options = PacketOptions.Checksum)
        {
            var packet = new Packet(bytes, options);
            if (packet.IsEncrypted)
            {
                throw new NotImplementedException("Packet encryption not supported");
            }
            if (packet.HasChecksum)
            {
                packet._checksum = Hasher.ComputeHash(packet._bytes);
            }
            return packet;
        }

        private Packet(byte[] bytes, PacketOptions options = PacketOptions.Checksum)
        {
            _options = options;
            _bytes = bytes;
        }

        public bool IsEncrypted
        {
            get { return (_options & PacketOptions.Encryted) > 0; }
        }

        public bool IsCompressed
        {
            get { return (_options & PacketOptions.Compressed) > 0; }
        }

        public bool HasChecksum
        {
            get { return (_options & PacketOptions.Checksum) > 0; }
        }

        public static Packet Read(Stream stream)
        {
            // Read header
            BinaryReader reader = new BinaryReader(stream);
            PacketOptions options = (PacketOptions)reader.ReadByte();
            int length = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(length);
            var packet = new Packet(bytes, options);
            if (packet.IsCompressed) packet._bytes = bytes.Decompress();
            if (packet.HasChecksum)
            {
                int checksumLength = reader.ReadInt16();
                packet._checksum = reader.ReadBytes(checksumLength);
                if (!packet.HasValidChecksum()) throw new InvalidDataException("Bad checksum reading packet");
            }
            return packet;
        }

        public void Write(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)_options);
            byte[] toWrite = IsCompressed ? _bytes.Compress() : _bytes;
            writer.Write(toWrite.Length);
            writer.Write(toWrite);
            if (HasChecksum)
            {
                writer.Write((short)_checksum.Length);
                writer.Write(_checksum);
            }
            writer.Flush();
        }

        private bool HasValidChecksum()
        {
            byte[] computedHash = Hasher.ComputeHash(_bytes);
            return computedHash.EqualsEx(_checksum);
        }

        public byte[] Bytes
        {
            get
            {
                return _bytes;
            }
        }

    }
}