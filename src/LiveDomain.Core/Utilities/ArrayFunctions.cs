using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core.Utilities
{
static class ArrayFunctions
{
public static IEnumerable<T> MergeSort<T>(T[][] arrays) where T : IComparable<T>
{
    return MergeSort(arrays, (a, b) => a.CompareTo(b));
}

public static IEnumerable<T> MergeSort<T>(T[][] arrays, Comparison<T> comparer)
{
    var queues = new List<NonDestructiveArrayQueue<T>>(arrays.Select(a => new NonDestructiveArrayQueue<T>(a)));
    while (queues.Count > 1)
    {
        queues.Sort((a, b) => comparer.Invoke(a.First(), b.First()));
        while (queues[0].Count > 0 && comparer.Invoke(queues[0].First(), queues[1].First()) <= 0)
        {
            yield return queues[0].Dequeue();
        }
        //remove empty lists
        queues.RemoveAll(q => q.Count == 0);
    }

    //empty the last list 
    foreach (T item in queues[0]) yield return item;

}
}
}
