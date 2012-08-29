using System;
using System.IO;
using System.Collections.Generic;
namespace LiveDomain.Core
{
    public interface ISerializer
    {
        object Clone(object graph);
        T Clone<T>(T graph);
        T Deserialize<T>(byte[] bytes);
        T Read<T>(Stream stream);
        IEnumerable<T> ReadToEnd<T>(Stream stream);
        byte[] Serialize(object graph);
        void Write(object graph, Stream stream);
    }
}
