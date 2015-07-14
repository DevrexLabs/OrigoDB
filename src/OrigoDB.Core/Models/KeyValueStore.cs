using System;
using System.Collections.Generic;


namespace OrigoDB.Core.Models
{
    [Serializable]
    public class KeyValueStore : Model
    {
        [Serializable]
        public class Node
        {
            public int Version { get; private set; }

            public object Item { get; private set; }

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
                    throw new Exception("Version mismatch");
                }
            }
        }

        readonly SortedDictionary<string,Node> _store 
            = new SortedDictionary<string, Node>(StringComparer.InvariantCultureIgnoreCase);

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
        public int Set(string key, object value, int? expectedVersion)
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

        public void Remove(string key, int? expectedVersion)
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
}