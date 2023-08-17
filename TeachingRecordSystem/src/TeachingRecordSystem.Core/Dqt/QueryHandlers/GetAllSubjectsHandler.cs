using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllSubjectsHandler : ICrmQueryHandler<GetAllSubjectsQuery, Subject[]>
{
    public async Task<Subject[]> Execute(GetAllSubjectsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = Subject.EntityLogicalName,
            ColumnSet = new ColumnSet(
                Subject.Fields.Description,
                Subject.Fields.Title)
        };

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<Subject>()).ToArray();
    }
}
