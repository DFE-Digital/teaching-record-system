namespace TeachingRecordSystem.Api;

public sealed class NoopAsyncDisposable : IAsyncDisposable
{
    private NoopAsyncDisposable()
    {
    }

    public static NoopAsyncDisposable Instance { get; } = new NoopAsyncDisposable();

    public ValueTask DisposeAsync() => default;
}
