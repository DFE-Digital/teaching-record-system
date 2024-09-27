using System.ServiceModel;
using Hangfire;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class SyncAllAlertsFromCrmJob
{
    private readonly ICrmServiceClientProvider _crmServiceClientProvider;
    private readonly TrsDataSyncHelper _trsDataSyncHelper;
    private readonly TrsDbContext _dbContext;
    private readonly IOptions<TrsDataSyncServiceOptions> _syncOptionsAccessor;

    public SyncAllAlertsFromCrmJob(
        ICrmServiceClientProvider crmServiceClientProvider,
        TrsDataSyncHelper trsDataSyncHelper,
        TrsDbContext dbContext,
        IOptions<TrsDataSyncServiceOptions> syncOptionsAccessor)
    {
        _crmServiceClientProvider = crmServiceClientProvider;
        _trsDataSyncHelper = trsDataSyncHelper;
        _dbContext = dbContext;
        _syncOptionsAccessor = syncOptionsAccessor;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var serviceClient = _crmServiceClientProvider.GetClient(TrsDataSyncService.CrmClientName);
        var columns = new ColumnSet(TrsDataSyncHelper.GetEntityInfoForModelType(TrsDataSyncHelper.ModelTypes.Alert).AttributeNames);

        var query = new QueryExpression(dfeta_sanction.EntityLogicalName)
        {
            ColumnSet = columns,
            Orders =
            {
                new OrderExpression(dfeta_sanction.PrimaryIdAttribute, OrderType.Ascending)
            },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            }
        };

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            EntityCollection result;
            try
            {
                result = await serviceClient.RetrieveMultipleAsync(query);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            await _trsDataSyncHelper.SyncAlerts(
                result.Entities.Select(e => e.ToEntity<dfeta_sanction>()).ToArray(),
                ignoreInvalid: _syncOptionsAccessor.Value.IgnoreInvalidData,
                createMigratedEvent: true,
                dryRun: true,
                cancellationToken);

            if (result.MoreRecords)
            {
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = result.PagingCookie;
            }
            else
            {
                break;
            }
        }
    }
}