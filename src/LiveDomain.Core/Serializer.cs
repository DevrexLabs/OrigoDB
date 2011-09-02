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
	public class Serializer
	{
		IFormatter _formatter;

		internal Serializer(IFormatter formatter)
		{
			_formatter = formatter;
		}

		internal void Write(object graph, Stream stream)
		{
			_formatter.Serialize(stream, graph);
		}

		internal T Read<T>(Stream stream)
		{
			return (T) _formatter.Deserialize(stream);
		}

		internal IEnumerable<T> ReadToEnd<T>(Stream stream)
		{
			while (stream.Position < stream.Length)
			{
				yield return Read<T>(stream);
			}
		}


		internal byte[] Serialize(object graph)
		{
			MemoryStream ms = new MemoryStream();
			_formatter.Serialize(ms, graph);
			return ms.ToArray();
		}

        internal object Clone(object graph)
        {
            return Deserialize<object>(Serialize(graph));
        }

		internal T Clone<T>(T graph)
		{
			return Deserialize<T>(Serialize(graph));
		}

		internal T Deserialize<T>(byte[] bytes)
		{
			MemoryStream ms = new MemoryStream(bytes);
			return (T) _formatter.Deserialize(ms);
		}
	}
}
