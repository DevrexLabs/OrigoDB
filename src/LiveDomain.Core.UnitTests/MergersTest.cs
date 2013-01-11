using System.Linq;
using System;
using NUnit.Framework;

namespace LiveDomain.Core.Test
{


    /// <summary>
    ///This is a test class for ArrayFunctionsTest and is intended
    ///to contain all ArrayFunctionsTest Unit Tests
    ///</summary>
    [TestFixture]
    public class MergersTest
    {

        [Test]
        public void OrderedArraysStreamed_results_are_ordered()
        {
            var arrays = RandomArrays(5);
            int[] sorted = Merge.OrderedArraysStreamed(arrays).ToArray();
            Assert.IsTrue(IsOrdered(sorted));
        }

        [Test]
        public void OrderedArraysStreamed_retains_correct_no_of_items()
        {
            var arrays = RandomArrays(12);
            int numItems = arrays.Sum(a => a.Length);
            int[] sorted = Merge.OrderedArraysStreamed(arrays).ToArray();
            Assert.AreEqual(numItems, sorted.Length);
        }

        [Test]
        public void OrderedArraysStreamed_results_are_ordered_with_comparer()
        {
            var arrays = RandomArrays(5);
            foreach (int[] array in arrays)
            {
                Array.Reverse(array);
            }
            Comparison<int> comparer = (a, b) => b.CompareTo(a);
            int[] sorted = Merge.OrderedArraysStreamed(arrays, comparer).ToArray();
            Assert.IsTrue(IsOrdered(sorted, comparer));
        }


        private int[][] RandomArrays(int numArrays)
        {
            int[][] arrays = new int[numArrays][];
            for (int i = 0; i < numArrays; i++)
            {
                arrays[i] = RandomArray();
                Array.Sort(arrays[i]);
            }
            return arrays;
        }


        private bool IsOrdered<T>(T[] array) where T : IComparable<T>
        {
            return IsOrdered(array, (a, b) => a.CompareTo(b));
        }

        private bool IsOrdered<T>(T[] array, Comparison<T> comparer )
        {
            //if any two adjacent elements are out of order, the array is not in order
            for (int i = 0; i < array.Length - 1; i++)
            {
                if (comparer.Invoke(array[i], array[i + 1]) > 0) return false;
            }
            return true;
        }

        Random random = new Random(42);

        private int[] RandomArray()
        {
            int size = random.Next() % 10;
            size++; //avoid size == 0
            int[] result = new int[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = random.Next() % 100;
            }
            return result;
        }
    }
}
