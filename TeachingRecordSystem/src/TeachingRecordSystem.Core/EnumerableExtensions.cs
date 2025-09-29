namespace TeachingRecordSystem.Core;

public static class EnumerableExtensions
{
    public static IReadOnlyCollection<T> AsReadOnly<T>(this IEnumerable<T> enumerable) =>
        enumerable is IReadOnlyCollection<T> roc ? roc : enumerable.ToArray();

    public static T First<T>(this IEnumerable<T> source, Func<T, bool> predicate, string failedErrorMessage) =>
        source.FirstOrDefault(predicate) ?? throw new InvalidOperationException(failedErrorMessage);

    public static T Single<T>(this IEnumerable<T> source, Func<T, bool> predicate, string failedErrorMessage) =>
        source.SingleOrDefault(predicate) ?? throw new InvalidOperationException(failedErrorMessage);

    public static IEnumerable<T[]> Permutations<T>(this IEnumerable<T> source, int length)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        return source.Permutations().Select(p => p.ToArray()).Where(c => c.Length == length);
    }

    public static string ToCommaDelimitedString(
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
#pragma warning disable format
            [] => string.Empty,
            [var only] => only,
#pragma warning restore format
            _ => string.Join(", ", valuesArray[..^2].Append(string.Join($" {finalValuesConjunction} ", valuesArray[^2..])))
        };
    }

    public static bool SequenceEqualIgnoringOrder<T>(this IEnumerable<T> first, IEnumerable<T> second)
        where T : IComparable
    {
        var firstArray = first.ToArray().OrderBy(s => s);
        var secondArray = second.ToArray().OrderBy(s => s);
        return firstArray.SequenceEqual(secondArray);
    }

    public static async IAsyncEnumerable<T> ToAsyncEnumerableAsync<T>(this Task<T[]> task)
    {
        var result = await task;

        foreach (var r in result)
        {
            yield return r;
        }
    }
}
