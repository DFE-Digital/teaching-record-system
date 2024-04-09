using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllActiveHeSubjectsHandler : ICrmQueryHandler<GetAllActiveHeSubjectsQuery, dfeta_hesubject[]>
{
    public async Task<dfeta_hesubject[]> Execute(GetAllActiveHeSubjectsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_hesubject.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_hesubject.Fields.dfeta_name,
                dfeta_hesubject.Fields.dfeta_Value)
        };

        queryExpression.Criteria.AddCondition(dfeta_hesubject.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_hesubjectState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_hesubject>()).ToArray();
    }
}
