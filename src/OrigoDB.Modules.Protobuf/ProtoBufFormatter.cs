using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using OrigoDB.Modules.Protobuf;
using ProtoBuf;
using System.Reflection;
using System.Threading;
using ProtoBuf.Meta;

namespace OrigoDB.Modules.ProtoBuf
{
    /// <summary>
    /// Provides functionality for formatting serialized objects
    /// using ProtoBuf serialization.
    /// </summary>
    public sealed class ProtoBufFormatter : IFormatter
    {
        private StreamingContext _context;
        private readonly Dictionary<string, Type> _typeLookup;
        //private RuntimeTypeModel _typeModel;
        private RuntimeTypeModelBuilder _modelBuilder;


        /// <summary>
        /// The associated serialization binder.
        /// </summary>
        public SerializationBinder Binder
        {
            get { return null; }
            set { /* Do nothing here since it's not used. */; }
        }

        /// <summary>
        /// The associated streaming context.
        /// </summary>
        public StreamingContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        /// <summary>
        /// The associated surrogate selector.
        /// </summary>
        public ISurrogateSelector SurrogateSelector
        {
            get { return null; }
            set { /* Do nothing here since it's not used. */; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrigoDB.Modules.ProtoBuf.ProtoBufFormatter"/> class.
        /// </summary>
        public ProtoBufFormatter(RuntimeTypeModelBuilder modelBuilder = null)
        {
            _context = new StreamingContext(StreamingContextStates.Persistence);
            _typeLookup = new Dictionary<string, Type>();
            _modelBuilder = modelBuilder ?? new RuntimeTypeModelBuilder();
        }

        /// <summary>
        /// Deserializes the data on the provided stream and 
        /// reconstitutes the graph of objects.
        /// </summary>
        /// <param name="stream">The stream that contains the data to deserialize.</param>
        /// <returns>The top object of the deserialized graph.</returns>
        public object Deserialize(Stream stream)
        {
            // Perform sanity checks.
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Read the type information from the stream.
            ProtoBufStreamHeader header = ProtoBufStreamHeader.Read(stream);
            string typeName = header.TypeName;

            // Already know this type?
            Type type = null;
            if (!_typeLookup.TryGetValue(typeName, out type))
            {
                /////////////////////////////////////////////////////////////////////////////////////////////
                // The Type.GetType only works if the requested type exists in the
                // currently executing assembly or in Mscorlib.dll, we therefore check
                // this first - and if that doesn't work - we check the whole app-domain.
                // Not the best idea since we potentially would do this every serialization
                // if we swallow the exception that is thrown when we can't resolve the type.
                // Hopefully, this is not something that is ignored by the consumer of the serializer...
                /////////////////////////////////////////////////////////////////////////////////////////////

                // The type did not exist in our cache.
                type = this.ResolveType(typeName);
                if (type == null)
                {
                    // We could not get the type representation.
                    string message = string.Format("Type with full name '{0}' could not be resolved.", typeName);
                    throw new ProtoBufFormatterException(message);
                }

                // Add the type to the cache.
                _typeLookup.Add(typeName, type);
            }

            EnsureCanHandle(type);

            // Deserialize the stream.
            return  _modelBuilder.TypeModel.DeserializeWithLengthPrefix(
                stream, 
                null, 
                type, 
                PrefixStyle.Fixed32BigEndian, 
                0);
        }

        /// <summary>
        /// Throw an exception unless we can handle the type
        /// </summary>
        /// <param name="type"></param>
        private void EnsureCanHandle(Type type)
        {
            _modelBuilder.Add(type);
        }

        /// <summary>
        /// Serializes an object, or graph of objects with the given root to the provided stream.
        /// </summary>
        /// <param name="stream">The stream where the formatter puts the serialized data. This stream can
        /// reference a variety of backing stores (such as files, network, memory, and so on).</param>
        /// <param name="graph">The object, or root of the object graph, to serialize.
        /// All child objects of this root object are automatically serialized.</param>
        public void Serialize(Stream stream, object graph)
        {
            // Perform sanity checks.
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (graph == null)
            {
                throw new ArgumentNullException("graph");
            }

            
            Type type = graph.GetType();
            EnsureCanHandle(type);

            string typeName = type.FullName;
            if (!_typeLookup.ContainsKey(typeName))
            {
                _typeLookup.Add(typeName, type);
            }

            

            // Create the header and write it to the stream.
            ProtoBufStreamHeader.Create(type).Write(stream);

            // Perform the actual ProtoBuf serialization.
            _modelBuilder.TypeModel.SerializeWithLengthPrefix(stream, graph, type, PrefixStyle.Fixed32BigEndian, 0, _context);
        }

        /// <summary>
        /// Returns whether or not a type is known to the formatter.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsKnownType(Type type)
        {
            return _typeLookup.ContainsKey(type.FullName);
        }

        private Type ResolveType(string typeName)
        {
            // The type did not exist in our cache.
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            // Try looking in the current application domain for the type.
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }


    }
}
