using System.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class MigrateInductionsFromCrmJob(
    ICrmServiceClientProvider crmServiceClientProvider,
    TrsDataSyncHelper trsDataSyncHelper,
    IOptions<TrsDataSyncServiceOptions> syncOptionsAccessor,
    ILogger<SyncAllInductionsFromCrmJob> logger)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var serviceClient = crmServiceClientProvider.GetClient(TrsDataSyncService.CrmClientName);
        var columns = new ColumnSet(TrsDataSyncHelper.GetEntityInfoForModelType(TrsDataSyncHelper.ModelTypes.Person).AttributeNames);

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

            await trsDataSyncHelper.MigrateInductionsAsync(
                result.Entities.Select(e => e.ToEntity<Contact>()).ToArray(),
                ignoreInvalid: syncOptionsAccessor.Value.IgnoreInvalidData,
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
