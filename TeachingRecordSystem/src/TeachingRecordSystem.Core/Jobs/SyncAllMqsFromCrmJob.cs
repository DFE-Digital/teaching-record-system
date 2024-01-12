using Hangfire;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class SyncAllMqsFromCrmJob
{
    private readonly ICrmServiceClientProvider _crmServiceClientProvider;
    private readonly TrsDataSyncHelper _trsDataSyncHelper;
    private readonly IOptions<TrsDataSyncServiceOptions> _syncOptionsAccessor;

    public SyncAllMqsFromCrmJob(
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
        var columns = new ColumnSet(TrsDataSyncHelper.GetEntityInfoForModelType(TrsDataSyncHelper.ModelTypes.MandatoryQualification).AttributeNames);

        var filter = new FilterExpression(LogicalOperator.And)
        {
            Conditions =
            {
                new ConditionExpression(dfeta_qualification.Fields.dfeta_Type, ConditionOperator.Equal, (int)dfeta_qualification_dfeta_Type.MandatoryQualification)
            }
        };

        var query = new QueryExpression(dfeta_qualification.EntityLogicalName)
        {
            ColumnSet = columns,
            Criteria = filter,
            Orders =
            {
                new OrderExpression(dfeta_qualification.Fields.CreatedOn, OrderType.Ascending)
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

            await _trsDataSyncHelper.SyncMandatoryQualifications(
                result.Entities.Select(e => e.ToEntity<dfeta_qualification>()).ToArray(),
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
