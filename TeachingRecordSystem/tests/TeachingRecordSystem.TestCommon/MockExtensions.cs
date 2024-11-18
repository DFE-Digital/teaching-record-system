using Moq.Language.Flow;

namespace TeachingRecordSystem.TestCommon;

public static class MockExtensions
{
    public static IReturnsResult<T> ReturnsAsyncEnumerable<T, TResult>(
            this ISetup<T, IAsyncEnumerable<TResult>> setup,
            params TResult[] items)
        where T : class
    {
        return setup.Returns(GetAsyncEnumerableAsync(items));

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        static async IAsyncEnumerable<R> GetAsyncEnumerableAsync<R>(IEnumerable<R> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
