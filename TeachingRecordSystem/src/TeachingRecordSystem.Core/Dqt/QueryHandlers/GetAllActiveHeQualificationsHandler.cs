using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllActiveHeQualificationsHandler : ICrmQueryHandler<GetAllActiveHeQualificationsQuery, dfeta_hequalification[]>
{
    public async Task<dfeta_hequalification[]> Execute(GetAllActiveHeQualificationsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_hequalification.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_hequalification.Fields.dfeta_name,
                dfeta_hequalification.Fields.dfeta_Value)
        };

        queryExpression.Criteria.AddCondition(dfeta_hequalification.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_hequalificationState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_hequalification>()).ToArray();
    }
}
