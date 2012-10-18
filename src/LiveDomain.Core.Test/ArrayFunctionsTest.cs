using System.Linq;
using LiveDomain.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace LiveDomain.Core.Test
{


    /// <summary>
    ///This is a test class for ArrayFunctionsTest and is intended
    ///to contain all ArrayFunctionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArrayFunctionsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void MergeSort_sorts()
        {
            var arrays = RandomArrays(5);
            int[] sorted = ArrayFunctions.MergeSort(arrays).ToArray();
            Assert.IsTrue(IsOrdered(sorted));
        }

        [TestMethod()]
        public void MergeSort_retains_correct_no_of_items()
        {
            var arrays = RandomArrays(12);
            int numItems = arrays.Sum(a => a.Length);
            int[] sorted = ArrayFunctions.MergeSort(arrays).ToArray();
            Assert.AreEqual(numItems, sorted.Length);
        }

        [TestMethod()]
        public void MergeSort_with_comparer_sorts()
        {
            var arrays = RandomArrays(5);
            foreach (int[] array in arrays)
            {
                Array.Reverse(array);
            }
            Comparison<int> comparer = (a, b) => b.CompareTo(a);
            int[] sorted = ArrayFunctions.MergeSort(arrays, comparer).ToArray();
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
