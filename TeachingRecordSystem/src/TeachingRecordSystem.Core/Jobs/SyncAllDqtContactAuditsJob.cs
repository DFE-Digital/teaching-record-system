using System.ServiceModel;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class SyncAllDqtContactAuditsJob(
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    TrsDataSyncHelper trsDataSyncHelper)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(),
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
                result = await organizationService.RetrieveMultipleAsync(query);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            await trsDataSyncHelper.SyncAuditAsync(Contact.EntityLogicalName, result.Entities.Select(e => e.Id), cancellationToken);

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
