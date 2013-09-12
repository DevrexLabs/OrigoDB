using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OrigoDB.Modules.ProtoBuf;

namespace Modules.ProtoBuf.Test.Framework
{
    internal static class SerializationHelper
    {
        internal static Stream Serialize<T>(T instance, ProtoBufFormatter formatter = null)
        {
            formatter = formatter ?? new ProtoBufFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, instance);
            stream.Position = 0;
            return stream;
        }

        internal static T Deserialize<T>(Stream stream, ProtoBufFormatter formatter = null)
        {
            formatter = formatter ?? new ProtoBufFormatter();
            return (T)formatter.Deserialize(stream);
        }
    }
}
