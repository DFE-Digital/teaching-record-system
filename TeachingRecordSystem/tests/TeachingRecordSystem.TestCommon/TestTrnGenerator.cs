using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class TestTrnGenerator(IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task<string> GenerateTrnAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var nextTrn = await dbContext
            .Database
            .SqlQueryRaw<Result>("SELECT \"fn_generate_trn\" as Value FROM fn_generate_trn()")
            .FirstOrDefaultAsync();

        if (nextTrn?.Value is null)
        {
            var trnRangeCount = await dbContext.TrnRanges.CountAsync();

            throw new InvalidOperationException($"Failed to generate a TRN ({trnRangeCount} TRN ranges).");
        }

        return nextTrn.Value.Value.ToString("D7");
    }

    private record Result(int? Value);
}

public class TestEventPublisher : IEventPublisher
{
    public Task PublishEventAsync(IEvent @event, ProcessContext processContext)
    {
        return Task.CompletedTask;
    }
}
