using System.Runtime.CompilerServices;

namespace TeachingRecordSystem.Core;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T[]> ChunkAsync<T>(this IAsyncEnumerable<T> source, int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);

        var chunk = new List<T>(size);

        await foreach (var item in source)
        {
            chunk.Add(item);

            if (chunk.Count == size)
            {
                yield return chunk.ToArray();
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
        {
            yield return chunk.ToArray();
        }
    }

#pragma warning disable VSTHRD200
    public static async IAsyncEnumerable<T> WhereAwait<T>(
#pragma warning restore VSTHRD200
        this IAsyncEnumerable<T> source,
        Func<T, CancellationToken, Task<bool>> predicate,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            if (await predicate(item, cancellationToken))
            {
                yield return item;
            }
        }
    }

#pragma warning disable VSTHRD200
    public static async IAsyncEnumerable<R> SelectAwait<T, R>(
#pragma warning restore VSTHRD200
        this IAsyncEnumerable<T> source,
        Func<T, CancellationToken, Task<R>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            yield return await selector(item, cancellationToken);
        }
    }
}
