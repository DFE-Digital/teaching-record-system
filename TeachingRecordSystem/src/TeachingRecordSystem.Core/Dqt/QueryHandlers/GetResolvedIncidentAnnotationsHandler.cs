using System.Runtime.CompilerServices;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetResolvedIncidentAnnotationsHandler : IEnumerableCrmQueryHandler<GetResolvedIncidentAnnotationsQuery, Annotation[]>
{
    public async IAsyncEnumerable<Annotation[]> ExecuteAsync(
        GetResolvedIncidentAnnotationsQuery query,
        IOrganizationServiceAsync organizationService,
        [EnumeratorCancellation] CancellationToken cancellationToken)
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

        queryExpression.PageInfo = new()
        {
            Count = 50,
            PageNumber = 1
        };

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        EntityCollection response;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            response = await organizationService.RetrieveMultipleAsync(queryExpression);

            var annotations = response.Entities.Select(e => e.ToEntity<Annotation>()).ToArray();
            if (annotations.Length > 0)
            {
                yield return annotations;
            }

            queryExpression.PageInfo.PageNumber++;
            queryExpression.PageInfo.PagingCookie = response.PagingCookie;
        }
        while (response.MoreRecords);
    }
}
