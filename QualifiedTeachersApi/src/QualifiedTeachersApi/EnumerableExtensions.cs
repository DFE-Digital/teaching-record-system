﻿namespace QualifiedTeachersApi;

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

    public static string ToCommaSeparatedString(
        this IEnumerable<string> values,
        string finalValuesConjunction = "and")
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (finalValuesConjunction is null)
        {
            throw new ArgumentNullException(nameof(finalValuesConjunction));
        }

        var valuesArray = values.ToArray();

        return valuesArray switch
        {
            [] => string.Empty,
            [var only] => only,
            _ => string.Join(", ", valuesArray[0..^2].Append(string.Join($" {finalValuesConjunction} ", valuesArray[^2..])))
        };
    }
}
