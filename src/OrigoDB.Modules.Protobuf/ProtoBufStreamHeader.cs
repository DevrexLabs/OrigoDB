using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace OrigoDB.Modules.ProtoBuf
{
    /// <summary>
    /// Encapsulates information about a <see cref="System.Type"/> 
    /// to simplify serializing and deserializing.
    /// </summary>
    public sealed class ProtoBufStreamHeader
    {
        private readonly string _typeName;
        private readonly byte[] _buffer;

        /// <summary>
        /// The name of the type.
        /// </summary>
        public string TypeName
        {
            get { return _typeName; }
        } 

        /// <summary>
        /// The length of the type in bytes.
        /// </summary>
        public int Length
        {
            get { return _buffer.Length - sizeof(Int16); }
        }

        /// <summary>
        /// The actual header that will be written to the stream.
        /// </summary>
        public byte[] Buffer
        {
            get { return _buffer; }
        } 

        private ProtoBufStreamHeader(string typeName, byte[] buffer)
        {
            // Perform sanity checks.
            if (typeName == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("Type name cannot be empty.", "typeName");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            _typeName = typeName;
            _buffer = buffer;
        }

        /// <summary>
        /// Creates a new <see cref="OrigoDB.Modules.ProtoBuf.ProtoBufStreamHeader"/>
        /// from the specified <see cref="System.Type"/>.
        /// </summary>
        /// <param name="type">The specified <see cref="System.Type"/>.</param>
        /// <returns>A <see cref="OrigoDB.Modules.ProtoBuf.ProtoBufStreamHeader"/> representing the <see cref="System.Type"/>.</returns>
        public static ProtoBufStreamHeader Create(Type type)
        {
            // Perform sanity checks.
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // Write the type information to the stream.
            byte[] typeNameBuffer = Encoding.UTF8.GetBytes(type.FullName);
            byte[] typeLengthBuffer = BitConverter.GetBytes((Int16)typeNameBuffer.Length);

            // Concatinate the buffers.
            byte[] buffer = new byte[typeLengthBuffer.Length + typeNameBuffer.Length];
            System.Buffer.BlockCopy(typeLengthBuffer, 0, buffer, 0, typeLengthBuffer.Length);
            System.Buffer.BlockCopy(typeNameBuffer, 0, buffer, typeLengthBuffer.Length, typeNameBuffer.Length);

            // Return the buffer.
            return new ProtoBufStreamHeader(type.FullName, buffer);
        }

        /// <summary>
        /// Reads a <see cref="OrigoDB.Modules.ProtoBuf.ProtoBufStreamHeader"/> 
        /// from a <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="System.IO.Stream"/> to be read from.</param>
        /// <returns>A <see cref="OrigoDB.Modules.ProtoBuf.ProtoBufStreamHeader"/> representing the serialized <see cref="System.Type"/>.</returns>
        public static ProtoBufStreamHeader Read(Stream stream)
        {
            // Perform sanity checks.
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Read the length of the type name from the stream.
            byte[] typeLengthBuffer = new byte[sizeof(Int16)];
            int readByteCount = stream.Read(typeLengthBuffer, 0, sizeof(Int16));
            Debug.Assert(readByteCount == sizeof(Int16));
            Int16 typeLength = BitConverter.ToInt16(typeLengthBuffer, 0);

            // Read the type name from the stream.
            var typeNameBuffer = new byte[typeLength];
            readByteCount = stream.Read(typeNameBuffer, 0, typeLength);
            Debug.Assert(readByteCount == typeLength);
            string typeName = Encoding.UTF8.GetString(typeNameBuffer);

            // Concatinate the buffers.
            byte[] buffer = new byte[typeLengthBuffer.Length + typeNameBuffer.Length];
            System.Buffer.BlockCopy(typeLengthBuffer, 0, buffer, 0, typeLengthBuffer.Length);
            System.Buffer.BlockCopy(typeNameBuffer, 0, buffer, typeLengthBuffer.Length, typeNameBuffer.Length);

            return new ProtoBufStreamHeader(typeName, buffer);
        }

        /// <summary>
        /// Writes the type information header to the stream.
        /// </summary>
        /// <param name="stream">The <see cref="System.Stream"/> to write to.</param>
        public void Write(Stream stream)
        {
            // Perform sanity checks.
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            stream.Write(_buffer, 0, _buffer.Length);
        }
    }
}
