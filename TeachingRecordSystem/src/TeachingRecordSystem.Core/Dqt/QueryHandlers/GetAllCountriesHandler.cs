using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllCountriesHandler : ICrmQueryHandler<GetAllCountriesQuery, dfeta_country[]>
{
    public async Task<dfeta_country[]> Execute(GetAllCountriesQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = dfeta_country.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_country.Fields.dfeta_name,
                dfeta_country.Fields.dfeta_Value)
        };

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<dfeta_country>()).ToArray();
    }
}
