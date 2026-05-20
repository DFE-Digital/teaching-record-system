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
}
