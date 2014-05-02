using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using OrigoDB.Core.Utilities;
using OrigoDB.Core.Storage;

namespace OrigoDB.Core
{

    public static class FormatterExtensions
    {
        public static long SizeOf(this IFormatter formatter, object graph)
        {
            Ensure.NotNull(formatter, "formatter");
            Stream stream = new ByteCountingNullStream();
            formatter.Serialize(stream, graph);
            return stream.Length;
        }

        public static T Read<T>(this IFormatter formatter, Stream stream)
        {
            Ensure.NotNull(formatter, "formatter");
            return (T) formatter.Deserialize(stream);
        }

        public static IEnumerable<T> ReadToEnd<T>(this IFormatter formatter, Stream stream)
        {
            Ensure.NotNull(formatter, "formatter");
            while (stream.Position < stream.Length)
            {
                yield return Read<T>(formatter, stream);
            }
        }

        public static T FromByteArray<T>(this IFormatter formatter, byte[] bytes)
        {
            Ensure.NotNull(formatter, "formatter");
            Ensure.NotNull(bytes, "bytes");
            var ms = new MemoryStream(bytes);
            return (T)formatter.Deserialize(ms);
        }

        /// <summary>
        /// Avoid corruption by serializing in memory and
        /// then writing to the underlying stream.
        /// </summary>
        public static void WriteBuffered(this IFormatter formatter, Stream stream, object graph)
        {
            Ensure.NotNull(formatter, "formatter");
            Ensure.NotNull(graph, "graph");
            Ensure.NotNull(stream, "stream");

            var memStream = new MemoryStream();
            formatter.Serialize(memStream, graph);
            memStream.Position = 0;
            memStream.CopyTo(stream);
        }


        public static byte[] ToByteArray(this IFormatter formatter, object graph)
        {
            if (graph == null) throw new ArgumentNullException("graph");
            var ms = new MemoryStream();
            formatter.Serialize(ms, graph);
            return ms.ToArray();
        }

        public static T Clone<T>(this IFormatter formatter, T graph)
        {
            Ensure.NotNull(formatter, "formatter");
            Ensure.NotNull(graph, "graph");
            
            var ms = new MemoryStream();
            formatter.Serialize(ms, graph);
            ms.Position = 0;
            return (T) formatter.Deserialize(ms);
        }
    }
}
