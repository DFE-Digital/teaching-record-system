using System.Reflection;
using Xunit;
using Xunit.v3;

namespace TeachingRecordSystem.TestCommon;

public class InitializeDbFixture : IAsyncLifetime, INotifyTestLifecycleAsync
{
    // An assembly can have several fixtures deriving from this one, and each of them is notified about every test, so
    // pick one to do the clearing rather than clearing once per fixture.
    private static InitializeDbFixture? _clearDbOwner;

    public InitializeDbFixture() => Interlocked.CompareExchange(ref _clearDbOwner, this, null);

    public DbHelper DbHelper => DbHelper.Instance;

    public virtual async ValueTask InitializeAsync()
    {
        await InitializeDbAsync();
    }

    protected Task InitializeDbAsync() => DbHelper.InitializeAsync();

    // Clearing the database is asynchronous, and this is the only hook xUnit offers that lets us await it — the
    // BeforeAfterTestAttribute this used to be had to block. Note it runs before the test class is constructed, whereas
    // that attribute ran after.
    public async ValueTask OnTestStartingAsync(IXunitTest test)
    {
        if (!ReferenceEquals(this, _clearDbOwner))
        {
            return;
        }

        if (test.TestCase.TestClass.Class.GetCustomAttribute<ClearDbBeforeTestAttribute>(inherit: true) is not ClearDbBeforeTestAttribute clearDb)
        {
            return;
        }

        if (!test.TestCase.TestCollection.DisableParallelization)
        {
            throw new InvalidOperationException("Tests must be inside a collection with DisableParallelization set to true.");
        }

        await clearDb.ClearAsync();
    }

    public ValueTask OnTestFinishedAsync(IXunitTest test) => ValueTask.CompletedTask;

#pragma warning disable CA1816
    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
#pragma warning restore CA1816
}
