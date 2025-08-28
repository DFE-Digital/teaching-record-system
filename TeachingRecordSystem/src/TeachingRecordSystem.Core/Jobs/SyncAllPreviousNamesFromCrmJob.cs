using System.ServiceModel;
using Hangfire;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class SyncAllPreviousNamesFromCrmJob(
    ICrmServiceClientProvider crmServiceClientProvider,
    TrsDataSyncHelper trsDataSyncHelper)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var serviceClient = crmServiceClientProvider.GetClient(TrsDataSyncHelper.CrmClientName);
        var columns = new ColumnSet(dfeta_previousname.Fields.dfeta_previousnameId, dfeta_previousname.Fields.dfeta_PersonId);

        var query = new QueryExpression(dfeta_previousname.EntityLogicalName)
        {
            ColumnSet = columns,
            Criteria =
            {
                Conditions =
                {
                    new ConditionExpression(dfeta_previousname.Fields.StateCode, ConditionOperator.Equal, 0)
                }
            },
            Orders =
            {
                new OrderExpression(dfeta_previousname.PrimaryIdAttribute, OrderType.Ascending)
            },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            }
        };

        var processedContactIds = new HashSet<Guid>();
        processedContactIds.Add(new("84e845f4-0eaf-e311-b8ed-005056822391"));  // Exclude (In)Sanity Test record

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

            var contactIds = result.Entities
                .Select(e => e.ToEntity<dfeta_previousname>())
                .Where(e => e.dfeta_PersonId is not null)
                .Select(e => e.dfeta_PersonId.Id)
                .Distinct()
                .Where(id => !processedContactIds.Contains(id))
                .ToArray();

            await trsDataSyncHelper.SyncPreviousNamesForContactsAsync(
                contactIds,
                dryRun: false,
                cancellationToken);

            foreach (var contactId in contactIds)
            {
                processedContactIds.Add(contactId);
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
    }
}
