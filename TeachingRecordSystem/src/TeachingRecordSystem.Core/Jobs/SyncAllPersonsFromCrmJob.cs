using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class SyncAllPersonsFromCrmJob
{
    private readonly ICrmServiceClientProvider _crmServiceClientProvider;
    private readonly TrsDataSyncHelper _trsDataSyncHelper;
    private readonly IOptions<TrsDataSyncServiceOptions> _syncOptionsAccessor;

    public SyncAllPersonsFromCrmJob(
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
        var columns = new ColumnSet(TrsDataSyncHelper.GetEntityInfoForModelType(TrsDataSyncHelper.ModelTypes.Person).AttributeNames);

        // Ensure this is kept in sync with the predicate in TrsDataSyncHelper.SyncContacts
        var filter = new FilterExpression(LogicalOperator.And)
        {
            Conditions =
            {
                new ConditionExpression(Contact.Fields.dfeta_TRN, ConditionOperator.NotNull),
                new ConditionExpression(Contact.Fields.dfeta_TRN, ConditionOperator.NotEqual, ""),
            }
        };

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = columns,
            Criteria = filter,
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

            await _trsDataSyncHelper.SyncPersons(
                result.Entities.Select(e => e.ToEntity<Contact>()).ToArray(),
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
