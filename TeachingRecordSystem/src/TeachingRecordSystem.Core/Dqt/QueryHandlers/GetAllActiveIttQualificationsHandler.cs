using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllActiveIttQualificationsHandler : ICrmQueryHandler<GetAllActiveIttQualificationsQuery, dfeta_ittqualification[]>
{
    public async Task<dfeta_ittqualification[]> ExecuteAsync(GetAllActiveIttQualificationsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_ittqualification.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_ittqualification.Fields.dfeta_name,
                dfeta_ittqualification.Fields.dfeta_Value)
        };

        queryExpression.Criteria.AddCondition(dfeta_ittqualification.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_ittqualificationState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_ittqualification>()).ToArray();
    }
}
