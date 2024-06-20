using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.FormFlow;

namespace TeachingRecordSystem.UiCommon.FormFlow.State;

public class DbWithHttpContextTransactionUserInstanceStateProvider(
    IHttpContextAccessor httpContextAccessor,
    IDbContextFactory<TrsDbContext> dbContextFactory,
    ICurrentUserIdProvider currentUserIdProvider,
    IClock clock,
    IOptions<JsonOptions> jsonOptionsAccessor) : DbUserInstanceStateProviderBase(clock, jsonOptionsAccessor)
{
    private const string HttpContextItemsDbContextKey = "_FormFlowDbContext";

    public override async Task CompleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var userId = currentUserIdProvider.GetCurrentUserId();
        await CompleteInstanceAsync(instanceId, stateType, userId, dbContext);
    }

    public async Task CommitChanges()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");

        if (TryGetDbContext(httpContext, out var dbContext))
        {
            if (dbContext.Database.CurrentTransaction is not null)
            {
                await dbContext.Database.CurrentTransaction.CommitAsync();
            }

            await dbContext.DisposeAsync();
        }
    }

    public override async Task<JourneyInstance> CreateInstanceAsync(
        JourneyInstanceId instanceId,
        Type stateType,
        object state,
        IReadOnlyDictionary<object, object>? properties)
    {
        var dbContext = await EnsureDbContext();
        var userId = currentUserIdProvider.GetCurrentUserId();
        return await CreateInstanceAsync(instanceId, stateType, state, properties, userId, dbContext);
    }

    public override async Task DeleteInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var dbContext = await EnsureDbContext();
        var userId = currentUserIdProvider.GetCurrentUserId();
        await DeleteInstanceAsync(instanceId, stateType, userId, dbContext);
    }

    public override async Task<JourneyInstance?> GetInstanceAsync(JourneyInstanceId instanceId, Type stateType)
    {
        var dbContext = await EnsureDbContext();
        var userId = currentUserIdProvider.GetCurrentUserId();
        return await GetInstanceAsync(instanceId, stateType, userId, dbContext);
    }

    public override async Task UpdateInstanceStateAsync(JourneyInstanceId instanceId, Type stateType, object state)
    {
        var dbContext = await EnsureDbContext();
        var userId = currentUserIdProvider.GetCurrentUserId();
        await UpdateInstanceStateAsync(instanceId, stateType, state, userId, dbContext);
    }

    private async Task<TrsDbContext> EnsureDbContext()
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");

        if (TryGetDbContext(httpContext, out var dbContext))
        {
            return dbContext;
        }

        dbContext = await dbContextFactory.CreateDbContextAsync();
        httpContext.Items.Add(HttpContextItemsDbContextKey, dbContext);

        await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        return dbContext;
    }

    private bool TryGetDbContext(HttpContext httpContext, [NotNullWhen(true)] out TrsDbContext? dbContext)
    {
        if (httpContext.Items.TryGetValue(HttpContextItemsDbContextKey, out var dbContextObj) && dbContextObj is TrsDbContext dbc)
        {
            dbContext = dbc;
            return true;
        }

        dbContext = default;
        return false;
    }
}
