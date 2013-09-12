using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core.Utilities
{
    /// <summary>
    /// Provided the ability to iterate an array in a queue like manner.
    /// Saves memory compared to using new Queue(array)
    /// </summary>
    public class NonDestructiveArrayQueue<T> : IEnumerable<T>
    {
        private T[] _items;
        private int _next;

        public NonDestructiveArrayQueue(T[] items)
        {
            _items = items;
            _next = 0;
        }

        public T Dequeue()
        {
            if (Count == 0) throw new InvalidOperationException("can't dequeue an empty queue");
            return _items[_next++];
        }

        public T First()
        {
            if (Count == 0) throw new InvalidOperationException("can't dequeue an empty queue");
            return _items[_next];
        }

        public int Count
        {
            get { return _items.Length - _next; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (Count > 0) yield return Dequeue();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
