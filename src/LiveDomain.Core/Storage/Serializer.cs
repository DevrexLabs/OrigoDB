using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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
			_formatter = formatter;
		}

		public void Write(object graph, Stream stream)
		{
			_formatter.Serialize(stream, graph);
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
