using System.ServiceModel;
using Hangfire;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class SyncAllInductionsFromCrmJob
{
    private readonly ICrmServiceClientProvider _crmServiceClientProvider;
    private readonly TrsDataSyncHelper _trsDataSyncHelper;
    private readonly IOptions<TrsDataSyncServiceOptions> _syncOptionsAccessor;

    public SyncAllInductionsFromCrmJob(
        ICrmServiceClientProvider crmServiceClientProvider,
        TrsDataSyncHelper trsDataSyncHelper,
        IOptions<TrsDataSyncServiceOptions> syncOptionsAccessor)
    {
        _crmServiceClientProvider = crmServiceClientProvider;
        _trsDataSyncHelper = trsDataSyncHelper;
        _syncOptionsAccessor = syncOptionsAccessor;
    }

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var serviceClient = _crmServiceClientProvider.GetClient(TrsDataSyncService.CrmClientName);
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
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            await _trsDataSyncHelper.SyncInductionsAsync(
                result.Entities.Select(e => e.ToEntity<Contact>()).ToArray(),
                ignoreInvalid: _syncOptionsAccessor.Value.IgnoreInvalidData,
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
