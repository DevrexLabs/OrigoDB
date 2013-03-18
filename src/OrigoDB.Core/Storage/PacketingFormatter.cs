using System.IO;
using System.Runtime.Serialization;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
    /// <summary>
    /// Decorator transforming graph to packet
    /// </summary>
    public class PacketingFormatter : IFormatter
    {
        PacketOptions _options;
        IFormatter _decoratedFormatter;


        public PacketingFormatter(IFormatter decoratedFormatter, PacketOptions options)
        {
            Ensure.NotNull(decoratedFormatter, "decoratedFormatter");
            _decoratedFormatter = decoratedFormatter;
            _options = options;
        }

        public void Serialize(Stream stream, object graph)
        {
            MemoryStream ms = new MemoryStream();
            _decoratedFormatter.Serialize(ms, graph);
            Packet packet = Packet.Create(ms.ToArray(), _options);
            packet.Write(stream);
        }

        public object Deserialize(Stream stream)
        {
            var packet = Packet.Read(stream);
            return _decoratedFormatter.Deserialize(new MemoryStream(packet.Bytes));
        }

        public SerializationBinder Binder
        {
            get { return _decoratedFormatter.Binder; }
            set { _decoratedFormatter.Binder = value; }
        }

        public StreamingContext Context
        {
            get { return _decoratedFormatter.Context; }
            set { _decoratedFormatter.Context = value; }
        }


        public ISurrogateSelector SurrogateSelector
        {
            get { return _decoratedFormatter.SurrogateSelector; }
            set { _decoratedFormatter.SurrogateSelector = value; }
        }
    }
}