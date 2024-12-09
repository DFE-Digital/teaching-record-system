using Moq.Language.Flow;

namespace TeachingRecordSystem.TestCommon;

public static class MockExtensions
{
    public static IReturnsResult<T> ReturnsAsyncEnumerable<T, TResult>(
            this ISetup<T, IAsyncEnumerable<TResult>> setup,
            params TResult[] items)
        where T : class
    {
        return setup.Returns(items.ToAsyncEnumerable());
    }
}
