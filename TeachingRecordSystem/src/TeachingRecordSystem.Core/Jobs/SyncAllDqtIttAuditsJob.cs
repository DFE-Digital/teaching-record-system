using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class SyncAllDqtIttAuditsJob(
    [FromKeyedServices(TrsDataSyncHelper.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    TrsDataSyncHelper trsDataSyncHelper,
    ILogger<SyncAllDqtIttAuditsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var query = new QueryExpression(dfeta_initialteachertraining.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(),
            Orders =
            {
                new OrderExpression("createdon", OrderType.Ascending),
                new OrderExpression(dfeta_initialteachertraining.PrimaryIdAttribute, OrderType.Ascending)
            },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            }
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
                dfeta_initialteachertraining.EntityLogicalName,
                result.Entities.Select(e => e.Id),
                skipIfExists: true,
                cancellationToken);

            if (fetched > 0 && fetched % 50000 == 0)
            {
                logger.LogWarning("Synced {Count} dfeta_initialteachertraining audit records.", fetched);
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

        logger.LogWarning("Synced {Count} dfeta_initialteachertraining audit records.", fetched);
    }
}
