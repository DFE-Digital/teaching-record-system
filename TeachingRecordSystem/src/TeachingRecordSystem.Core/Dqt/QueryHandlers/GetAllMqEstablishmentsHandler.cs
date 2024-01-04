using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllMqEstablishmentsHandler : ICrmQueryHandler<GetAllMqEstablishmentsQuery, dfeta_mqestablishment[]>
{
    public async Task<dfeta_mqestablishment[]> Execute(GetAllMqEstablishmentsQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.And);

        var queryExpression = new QueryExpression
        {
            EntityName = dfeta_mqestablishment.EntityLogicalName,
            ColumnSet = new ColumnSet(
                dfeta_mqestablishment.PrimaryIdAttribute,
                dfeta_mqestablishment.Fields.dfeta_name,
                dfeta_mqestablishment.Fields.dfeta_Value),
            Criteria = filter,
        };

        var request = new RetrieveMultipleRequest
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        return response.Entities.Select(x => x.ToEntity<dfeta_mqestablishment>()).ToArray();
    }
}
