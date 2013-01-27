using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using LiveDomain.Core.Migrations;
using LiveDomain.Core.Utilities;
using LiveDomain.Core.Storage;

namespace LiveDomain.Core
{

	/// <summary>
	/// Provides serialization using an IFormatter
	/// </summary>
	internal class Serializer : ISerializer
	{
		IFormatter _formatter;

		internal Serializer(IFormatter formatter)
		{
            Ensure.NotNull(formatter, "formatter");
            formatter.Binder = new CustomBinder();
			_formatter = formatter;
		}

        public long SizeOf(object graph)
        {
            Stream stream = new ByteCountingNullStream();
            _formatter.Serialize(stream, graph);
            return stream.Length;

        }

        /// <summary>
        /// Avoid corruption by serializing in memory and
        /// then writing to the underlying stream.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="stream"></param>
		public void Write(object graph, Stream stream)
		{
            MemoryStream memStream = new MemoryStream();
            _formatter.Serialize(memStream, graph);
		    memStream.Position = 0;
            memStream.CopyTo(stream);
		}

        public T Read<T>(Stream stream)
		{
			return (T) _formatter.Deserialize(stream);
		}

        public IEnumerable<T> ReadToEnd<T>(Stream stream)
		{
			while (stream.Position < stream.Length)
			{
				yield return Read<T>(stream);
			}
		}


        public byte[] Serialize(object graph)
		{
            if (graph == null) throw new ArgumentNullException("graph");
			MemoryStream ms = new MemoryStream();
			_formatter.Serialize(ms, graph);
			return ms.ToArray();
		}

        public object Clone(object graph)
        {
            if (graph == null) throw new ArgumentNullException("graph");
            return Deserialize<object>(Serialize(graph));
        }

        public T Clone<T>(T graph)
		{
			return Deserialize<T>(Serialize(graph));
		}

        public T Deserialize<T>(byte[] bytes)
		{
			MemoryStream ms = new MemoryStream(bytes);
			return (T) _formatter.Deserialize(ms);
		}
	}
}
