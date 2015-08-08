using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using OrigoDB.Core.Utilities;


namespace OrigoDB.Core.Types
{
    [Serializable]
    public class KeyValueStore : Model
    {
        [Serializable]
        public class Node
        {
            public int Version { get; private set; }

            public object Item { get; internal set; }

            internal int BumpAndSet(object item)
            {
                Item = item;
                return ++Version;
            }

            internal Node()
            {
            }

            public void ExpectVersion(int? version)
            {
                if (version.HasValue && Version != version.Value)
                {
                    throw new CommandAbortedException("Version mismatch");
                }
            }
        }

        readonly SortedDictionary<string,Node> _store 
            = new SortedDictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Retrieve an object from the store
        /// </summary>
        /// <param name="key">case insensitive string key</param>
        /// <returns>the requested object or throws an exception</returns>
        public Node Get(string key)
        {
            Node node;
            if (_store.TryGetValue(key, out node)) return node;
            throw new KeyNotFoundException("No such key: [" + key + "]");
        }

        /// <summary>
        /// Put a value in the store
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expectedVersion">must match current object, 0 for new</param>
        /// <returns>the version assigned to the inserted object</returns>
        [Command]
        public int Set(string key, object value, int? expectedVersion = null)
        {
            Node node;
            if (!_store.TryGetValue(key, out node))
            {
                node = new Node();
                node.ExpectVersion(expectedVersion);
                _store[key] = node;
            }
            else node.ExpectVersion(expectedVersion);
            return node.BumpAndSet(value);
        }

        public void Remove(string key, int? expectedVersion = null)
        {
            Node node; 
            if (_store.TryGetValue(key, out node))
            {
                node.ExpectVersion(expectedVersion);
            }
            else throw new KeyNotFoundException("Key [" + key + "]");
            _store.Remove(key);
        }
    }

    /// <summary>
    /// A wrapper for KeyValueStore that serializes/deserializes values to byte array.
    /// </summary>
    public class KeyValueStoreClient
    {
        private readonly IFormatter _formatter;
        private readonly KeyValueStore _store;
 
        public KeyValueStoreClient(KeyValueStore store, IFormatter formatter)
        {
            Ensure.NotNull(store, "store");
            Ensure.NotNull(formatter, "formatter");
            _store = store;
            _formatter = formatter;
        }

        public void Set(string key, object value, int? expectedVersion)
        {
            var bytes = _formatter.ToByteArray(value);
            _store.Set(key, bytes, expectedVersion);
        }

        public KeyValueStore.Node Get(string key)
        {
            var node = _store.Get(key);
            node.Item = _formatter.FromByteArray<object>((byte[]) node.Item);
            return node;
        }

        public void Remove(string key, int? expectedVersion)
        {
            _store.Remove(key, expectedVersion);
        }
    }
}