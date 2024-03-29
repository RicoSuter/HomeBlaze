using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartGlass.Common
{
    /// <summary>
    /// Enumerable extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> EnumerableOf<T>(params T[] items)
        {
            return items;
        }

        public static IEnumerable<int> OfRange(int first, int last)
        {
            if (first > last)
            {
                throw new ArgumentException("The start of the range must be equal to or less than the end.");
            }

            for (var i = first; i <= last; i++)
            {
                yield return i;
            }
        }

        public static IEnumerable<T> EnumerableOf<T>(T item, int size)
        {
            if (size == 0)
            {
                return new T[] { };
            }

            return OfRange(0, size - 1).Select(i => item).ToArray();
        }
    }
}
