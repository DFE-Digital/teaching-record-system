using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllIttSubjectsHandler : ICrmQueryHandler<GetAllIttSubjectsQuery, dfeta_ittsubject[]>
{
    public async Task<dfeta_ittsubject[]> ExecuteAsync(GetAllIttSubjectsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_ittsubject.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_hesubject.Fields.dfeta_name,
                dfeta_hesubject.Fields.dfeta_Value)
        };

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_ittsubject>()).ToArray();
    }
}
