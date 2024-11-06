namespace TeachingRecordSystem.Core;

public static class TaskEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<T[]> task)
    {
        var result = await task;

        foreach (var r in result)
        {
            yield return r;
        }
    }
}