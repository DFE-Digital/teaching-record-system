using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllActiveEarlyYearsStatusesHandler : ICrmQueryHandler<GetAllActiveEarlyYearsStatusesQuery, dfeta_earlyyearsstatus[]>
{
    public async Task<dfeta_earlyyearsstatus[]> Execute(GetAllActiveEarlyYearsStatusesQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_earlyyearsstatus.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_earlyyearsstatus.Fields.dfeta_name,
                dfeta_earlyyearsstatus.Fields.dfeta_Value)
        };
        queryExpression.Criteria.AddCondition(dfeta_earlyyearsstatus.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_earlyyearsStatusState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_earlyyearsstatus>()).ToArray();
    }
}
