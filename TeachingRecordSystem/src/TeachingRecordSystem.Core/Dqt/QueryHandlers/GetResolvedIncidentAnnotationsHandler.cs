using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetResolvedIncidentAnnotationsHandler : ICrmQueryHandler<GetResolvedIncidentAnnotationsQuery, Annotation[]>
{
    public async Task<Annotation[]> Execute(GetResolvedIncidentAnnotationsQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = Annotation.EntityLogicalName,
            ColumnSet = query.ColumnSet
        };

        var documentLink = new LinkEntity(
            Annotation.EntityLogicalName,
            dfeta_document.EntityLogicalName,
            Annotation.Fields.ObjectId,
            dfeta_document.PrimaryIdAttribute,
            JoinOperator.Inner);
        queryExpression.LinkEntities.Add(documentLink);

        var incidentLink = new LinkEntity(
            dfeta_document.EntityLogicalName,
            Incident.EntityLogicalName,
            dfeta_document.Fields.dfeta_CaseId,
            Incident.PrimaryIdAttribute,
            JoinOperator.Inner);
        incidentLink.LinkCriteria.AddCondition(Incident.Fields.StateCode, ConditionOperator.Equal, (int)IncidentState.Active);
        incidentLink.LinkCriteria.AddCondition(Incident.Fields.ModifiedOn, ConditionOperator.LessThan, query.ModifiedBefore);
        incidentLink.LinkCriteria.AddCondition(Incident.Fields.SubjectId, ConditionOperator.In, query.SubjectIds.Cast<object>().ToArray());
        documentLink.LinkEntities.Add(incidentLink);

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<Annotation>()).ToArray();
    }
}
