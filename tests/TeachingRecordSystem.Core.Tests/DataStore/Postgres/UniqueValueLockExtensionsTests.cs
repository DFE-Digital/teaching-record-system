using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres;

public class UniqueValueLockExtensionsTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    private const string IndexName = "ix_test";

    // Long enough that a lock that isn't blocked would have been acquired
    private static readonly TimeSpan _blockedTimeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public async Task AcquireUniqueValueLockAsync_NoTransaction_Throws()
    {
        // Arrange
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        // Act
        var exception = await Record.ExceptionAsync(() => dbContext.AcquireUniqueValueLockAsync(IndexName, Guid.NewGuid().ToString()));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task AcquireUniqueValueLockAsync_SameValueLockedByAnotherTransaction_WaitsUntilThatTransactionEnds()
    {
        // Arrange
        var value = Guid.NewGuid().ToString();

        await using var firstDbContext = await DbContextFactory.CreateDbContextAsync();
        await using var firstTransaction = await firstDbContext.Database.BeginTransactionAsync();
        await firstDbContext.AcquireUniqueValueLockAsync(IndexName, value);

        await using var secondDbContext = await DbContextFactory.CreateDbContextAsync();
        await using var secondTransaction = await secondDbContext.Database.BeginTransactionAsync();

        // Act
        var secondLock = secondDbContext.AcquireUniqueValueLockAsync(IndexName, value);
        var whileFirstTransactionOpen = await Record.ExceptionAsync(() => secondLock.WaitAsync(_blockedTimeout));

        await firstTransaction.CommitAsync();
        var afterFirstTransactionEnded = await Record.ExceptionAsync(() => secondLock.WaitAsync(TimeSpan.FromSeconds(10)));

        // Assert
        Assert.IsType<TimeoutException>(whileFirstTransactionOpen);
        Assert.Null(afterFirstTransactionEnded);
    }

    [Fact]
    public async Task AcquireUniqueValueLockAsync_DifferentValueLockedByAnotherTransaction_DoesNotWait()
    {
        // Arrange
        await using var firstDbContext = await DbContextFactory.CreateDbContextAsync();
        await using var firstTransaction = await firstDbContext.Database.BeginTransactionAsync();
        await firstDbContext.AcquireUniqueValueLockAsync(IndexName, Guid.NewGuid().ToString());

        await using var secondDbContext = await DbContextFactory.CreateDbContextAsync();
        await using var secondTransaction = await secondDbContext.Database.BeginTransactionAsync();

        // Act
        var exception = await Record.ExceptionAsync(
            () => secondDbContext.AcquireUniqueValueLockAsync(IndexName, Guid.NewGuid().ToString()).WaitAsync(TimeSpan.FromSeconds(10)));

        // Assert
        Assert.Null(exception);
    }
}
