using System.ServiceModel;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class SyncAllInductionsFromCrmJob(
        ICrmServiceClientProvider crmServiceClientProvider,
        TrsDataSyncHelper trsDataSyncHelper,
        IOptions<TrsDataSyncServiceOptions> syncOptionsAccessor,
        ILogger<SyncAllInductionsFromCrmJob> logger)
{
    public async Task ExecuteAsync(bool createMigratedEvent, bool dryRun, CancellationToken cancellationToken)
    {
        const int pageSize = 500;

        var serviceClient = crmServiceClientProvider.GetClient(TrsDataSyncService.CrmClientName);
        var columns = new ColumnSet(TrsDataSyncHelper.GetEntityInfoForModelType(TrsDataSyncHelper.ModelTypes.Induction).AttributeNames);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = columns,
            Orders =
            {
                new OrderExpression(Contact.PrimaryIdAttribute, OrderType.Ascending)
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
                logger.LogWarning("Hit CRM service limits; error code: {ErrorCode}.  Retrying after {retryAfter} seconds.", fex.Detail.ErrorCode, retryAfter.TotalSeconds);
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            await trsDataSyncHelper.SyncInductionsAsync(
                result.Entities.Select(e => e.ToEntity<Contact>()).ToArray(),
                syncAudit: false,
                ignoreInvalid: syncOptionsAccessor.Value.IgnoreInvalidData,
                createMigratedEvent,
                dryRun,
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
