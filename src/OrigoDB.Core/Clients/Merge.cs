using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{

    /// <summary>
    /// Helper functions for PartitionClient result merging
    /// </summary>
    public static class Merge
    {

        public static T[] Arrays<T>(T[][] arrays)
        {
            return ArraysStreamed(arrays).ToArray();
        }

        public static IEnumerable<T> ArraysStreamed<T>(T[][] arrays)
        {
            return arrays.SelectMany(a => a);
        }

        /// <summary>
        /// Merge an array of ordered arrays into a single ordered array
        /// </summary>
        public static T[]  OrderedArrays<T>(T[][] arrays) where T : IComparable<T>
        {
            return OrderedArraysStreamed(arrays).ToArray();
        }


        public static T[] OrderedArrays<T>(T[][] arrays, Comparison<T> comparer) 
        {
            return OrderedArraysStreamed(arrays, comparer).ToArray();
        }

        /// <summary>
        /// Merge an array of ordered arrays returning an iterator
        /// </summary>
        public static IEnumerable<T> OrderedArraysStreamed<T>(T[][] arrays) where T : IComparable<T>
        {
            return OrderedArraysStreamed(arrays, (a, b) => a.CompareTo(b));
        }

        /// <summary>
        /// Merge an array of ordered arrays using the provided comparer and returning an iterator
        /// </summary>
        public static IEnumerable<T> OrderedArraysStreamed<T>(T[][] arrays, Comparison<T> comparer)
        {
            var queues = new List<NonDestructiveArrayQueue<T>>(arrays.Select(a => new NonDestructiveArrayQueue<T>(a)));

            Comparison<NonDestructiveArrayQueue<T>> queueComparer = (a, b) => comparer.Invoke(a.First(), b.First());
            
            queues.Sort(queueComparer);
            while (queues.Count > 1)
            {
                while (queues[0].Count > 0 && queueComparer.Invoke(queues[0], queues[1]) <= 0)
                {
                    yield return queues[0].Dequeue();
                }

                //resort the list of queues by moving the first queue down the list
                if (queues[0].Count > 0)
                {
                    int i = 0;
                    while(i < queues.Count - 1)
                    {
                        if (queueComparer.Invoke(queues[i], queues[i + 1]) <= 0) break;
                        Swap(queues, i, i + 1);
                        i++;
                    }
                }
                //remove empty lists
                queues.RemoveAll(q => q.Count == 0);
            }

            //empty the last list 
            foreach (T item in queues[0]) yield return item;

        }

        private static void Swap<T>(List<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
