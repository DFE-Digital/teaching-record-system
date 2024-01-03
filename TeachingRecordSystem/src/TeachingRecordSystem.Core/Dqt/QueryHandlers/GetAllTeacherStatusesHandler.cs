using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllTeacherStatusesHandler : ICrmQueryHandler<GetAllTeacherStatusesQuery, dfeta_teacherstatus[]>
{
    public async Task<dfeta_teacherstatus[]> Execute(GetAllTeacherStatusesQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_teacherstatus.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_teacherstatus.Fields.dfeta_name,
                dfeta_teacherstatus.Fields.dfeta_QTSDateRequired,
                dfeta_teacherstatus.Fields.dfeta_Value)
        };

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_teacherstatus>()).ToArray();
    }
}
