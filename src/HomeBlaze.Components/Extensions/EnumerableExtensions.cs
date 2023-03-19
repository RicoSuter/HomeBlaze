using System.Collections.Generic;
using System;

namespace HomeBlaze.Components.Extensions
{
    public static class EnumerableExtensions
    {
        public static decimal WeightedAverage<TItem>(this IEnumerable<TItem> items, Func<TItem, decimal> valueSelector, Func<TItem, decimal> weightSelector)
        {
            var totalWeight = 0.0m;
            var totalValue = 0.0m;
            foreach (var item in items)
            {
                var weight = weightSelector(item);
                var value = valueSelector(item);

                totalValue += value * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? totalValue / totalWeight : 0;
        }

        public static IEnumerable<(T First, T Second)> GetSegments<T>(this IEnumerable<T> items)
        {
            var previous = default(T);
            foreach (var coordinate in items)
            {
                if (previous != null)
                {
                    yield return (previous, coordinate);
                }

                previous = coordinate;
            }
        }
    }
}