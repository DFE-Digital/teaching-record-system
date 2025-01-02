using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class SyncDqtContactAuditsMopUpJob(
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    TrsDataSyncHelper trsDataSyncHelper,
    ILogger<SyncDqtContactAuditsMopUpJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var changedSince = new DateTime(2024, 12, 24);
        const int pageSize = 1000;

        var filter = new FilterExpression();
        filter.AddCondition(Contact.Fields.ModifiedOn, ConditionOperator.GreaterEqual, changedSince);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(),
            Orders =
            {
                new OrderExpression(Contact.Fields.CreatedOn, OrderType.Ascending),
                new OrderExpression(Contact.PrimaryIdAttribute, OrderType.Ascending)
            },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            },
            Criteria = filter
        };

        var fetched = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            EntityCollection result;
            try
            {
                result = await organizationService.RetrieveMultipleAsync(query);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            fetched += result.Entities.Count;

            await trsDataSyncHelper.SyncAuditAsync(
                Contact.EntityLogicalName,
                result.Entities.Select(e => e.Id),
                skipIfExists: true,
                cancellationToken);

            if (fetched > 0 && fetched % 50000 == 0)
            {
                logger.LogWarning("Synced {Count} contact audit records.", fetched);
            }

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

        logger.LogWarning("Synced {Count} contact audit records.", fetched);
    }
}
