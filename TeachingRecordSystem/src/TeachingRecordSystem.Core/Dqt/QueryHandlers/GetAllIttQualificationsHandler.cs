using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllIttQualificationsHandler : ICrmQueryHandler<GetAllIttQualificationsQuery, dfeta_ittqualification[]>
{
    public async Task<dfeta_ittqualification[]> ExecuteAsync(GetAllIttQualificationsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_ittqualification.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_ittqualification.Fields.dfeta_name,
                dfeta_ittqualification.Fields.dfeta_Value,
                dfeta_ittqualification.Fields.StateCode)
        };

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_ittqualification>()).ToArray();
    }
}
