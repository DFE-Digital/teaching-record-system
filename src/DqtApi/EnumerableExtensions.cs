using System;
using System.Collections.Generic;
using System.Linq;

namespace DqtApi
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T[]> GetCombinations<T>(this IEnumerable<T> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var array = source.ToArray();

            return Enumerable
                .Range(0, 1 << (array.Length))
                .Select(index => array
                   .Where((v, i) => (index & (1 << i)) != 0)
                   .ToArray());
        }

        public static IEnumerable<T[]> GetCombinations<T>(this IEnumerable<T> source, int length)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return GetCombinations(source).Where(c => c.Length == length);
        }
    }
}
