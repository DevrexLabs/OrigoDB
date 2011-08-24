using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace TimeTracker.Core
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Gets the next id for a list. The method gets the last item in the IEnumerable 
        /// and returns an increment of one (1) on the given id property. The method assumes that the list is sorted (ascending) by id.
        /// </summary>
        /// <typeparam name="L">Any type of object that contains an id property of type int</typeparam>
        /// <param name="list">An object that implements the IEnumerable interface</param>
        /// <param name="idGetter">An object that implements the IEnumerable interface</param> 
        /// <returns>An increment of one (1) on the given id property</returns>
        public static int GetNextId<L>(this IEnumerable<L> list, Func<L, int> idGetter)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list can not be null");
            }

            if (idGetter == null)
            {
                throw new ArgumentNullException("idGetter can not be null");
            }

            L lastItem = list.LastOrDefault();
            return lastItem == null ? 0 : idGetter.Invoke(lastItem) + 1;
        }
    }
}
