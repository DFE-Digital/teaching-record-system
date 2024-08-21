using System.Runtime.CompilerServices;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetNonOpenTaskAnnotationsHandler : IEnumerableCrmQueryHandler<GetNonOpenTaskAnnotationsQuery, Annotation[]>
{
    public async IAsyncEnumerable<Annotation[]> Execute(
        GetNonOpenTaskAnnotationsQuery query,
        IOrganizationServiceAsync organizationService,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var queryExpression = new QueryExpression()
        {
            EntityName = Annotation.EntityLogicalName,
            ColumnSet = query.ColumnSet
        };

        var taskLink = new LinkEntity(
            Annotation.EntityLogicalName,
            CrmTask.EntityLogicalName,
            Annotation.Fields.ObjectId,
            CrmTask.PrimaryIdAttribute,
            JoinOperator.Inner);
        taskLink.LinkCriteria.AddCondition(CrmTask.Fields.StateCode, ConditionOperator.NotEqual, (int)TaskState.Open);
        taskLink.LinkCriteria.AddCondition(CrmTask.Fields.ModifiedOn, ConditionOperator.LessThan, query.ModifiedBefore);
        taskLink.LinkCriteria.AddCondition(CrmTask.Fields.Subject, ConditionOperator.In, query.Subjects.Cast<object>().ToArray());
        queryExpression.LinkEntities.Add(taskLink);

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
