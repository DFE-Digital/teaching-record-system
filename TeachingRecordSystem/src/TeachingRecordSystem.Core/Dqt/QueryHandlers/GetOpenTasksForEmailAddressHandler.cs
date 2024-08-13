using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetOpenTasksForEmailAddressHandler : ICrmQueryHandler<GetOpenTasksForEmailAddressQuery, CrmTask[]>
{
    public async Task<CrmTask[]> Execute(GetOpenTasksForEmailAddressQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = CrmTask.EntityLogicalName,
            ColumnSet = new ColumnSet(
                            CrmTask.Fields.dfeta_EmailAddress, CrmTask.Fields.StateCode)
        };
        queryExpression.Criteria.AddCondition(CrmTask.Fields.dfeta_EmailAddress, ConditionOperator.Equal, query.EmailAddress);
        queryExpression.Criteria.AddCondition(CrmTask.Fields.StateCode, ConditionOperator.Equal, (int)TaskState.Open);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        return response.Entities.Select(e => e.ToEntity<CrmTask>()).ToArray();
    }
}
