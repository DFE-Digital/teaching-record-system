using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllActiveSanctionCodesHandler : ICrmQueryHandler<GetAllActiveSanctionCodesQuery, dfeta_sanctioncode[]>
{
    public async Task<dfeta_sanctioncode[]> Execute(GetAllActiveSanctionCodesQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_sanctioncode.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_sanctioncode.Fields.dfeta_sanctioncodeId,
                dfeta_sanctioncode.Fields.dfeta_Value,
                dfeta_sanctioncode.Fields.dfeta_name)
        };

        queryExpression.Criteria.AddCondition(dfeta_sanctioncode.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_sanctioncodeState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_sanctioncode>()).ToArray();
    }
}
