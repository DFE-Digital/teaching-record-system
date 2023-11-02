using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class SyncAllContactsFromCrmJob
{
    private readonly ICrmServiceClientProvider _crmServiceClientProvider;
    private readonly TrsDataSyncHelper _trsDataSyncHelper;
    private readonly IOptions<TrsDataSyncServiceOptions> _syncOptionsAccessor;

    public SyncAllContactsFromCrmJob(
        ICrmServiceClientProvider crmServiceClientProvider,
        TrsDataSyncHelper trsDataSyncHelper,
        IOptions<TrsDataSyncServiceOptions> syncOptionsAccessor)
    {
        _crmServiceClientProvider = crmServiceClientProvider;
        _trsDataSyncHelper = trsDataSyncHelper;
        _syncOptionsAccessor = syncOptionsAccessor;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var serviceClient = _crmServiceClientProvider.GetClient(TrsDataSyncService.CrmClientName);
        var columns = new ColumnSet(_trsDataSyncHelper.GetSyncedAttributeNames(Contact.EntityLogicalName));

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = columns,
            NoLock = true,  // Any changes that we miss because of this be picked up by the change log sync
            Orders =
            {
                new OrderExpression(Contact.Fields.CreatedOn, OrderType.Ascending)
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

            var result = await serviceClient.RetrieveMultipleAsync(query);

            await _trsDataSyncHelper.SyncEntities(
                Contact.EntityLogicalName,
                result.Entities,
                ignoreInvalid: _syncOptionsAccessor.Value.IgnoreInvalidData,
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
