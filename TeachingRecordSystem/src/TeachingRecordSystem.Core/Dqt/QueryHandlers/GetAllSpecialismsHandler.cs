using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllSpecialismsHandler : ICrmQueryHandler<GetAllSpecialismsQuery, dfeta_specialism[]>
{
    public async Task<dfeta_specialism[]> Execute(GetAllSpecialismsQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.And);

        var queryExpression = new QueryExpression
        {
            EntityName = dfeta_specialism.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_specialism.PrimaryIdAttribute,
                dfeta_specialism.Fields.dfeta_name,
                dfeta_specialism.Fields.dfeta_Value),
            Criteria = filter,
        };

        var request = new RetrieveMultipleRequest
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        return response.Entities.Select(x => x.ToEntity<dfeta_specialism>()).ToArray();
    }
}
