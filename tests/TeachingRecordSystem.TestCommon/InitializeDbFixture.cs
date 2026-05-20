using Xunit;

namespace TeachingRecordSystem.TestCommon;

public class InitializeDbFixture : IAsyncLifetime
{
    public DbHelper DbHelper => DbHelper.Instance;

    public virtual async ValueTask InitializeAsync()
    {
        await InitializeDbAsync();
    }

    protected Task InitializeDbAsync() => DbHelper.InitializeAsync();

#pragma warning disable CA1816
    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
#pragma warning restore CA1816
}
