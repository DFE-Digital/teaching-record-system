using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllActiveIttSubjectsHandler : ICrmQueryHandler<GetAllActiveIttSubjectsQuery, dfeta_ittsubject[]>
{
    public async Task<dfeta_ittsubject[]> ExecuteAsync(GetAllActiveIttSubjectsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_ittsubject.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_hesubject.Fields.dfeta_name,
                dfeta_hesubject.Fields.dfeta_Value)
        };

        queryExpression.Criteria.AddCondition(dfeta_ittsubject.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_ittsubjectState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_ittsubject>()).ToArray();
    }
}
