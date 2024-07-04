using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetNonOpenTaskAnnotationsHandler : ICrmQueryHandler<GetNonOpenTaskAnnotationsQuery, Annotation[]>
{
    public async Task<Annotation[]> Execute(GetNonOpenTaskAnnotationsQuery query, IOrganizationServiceAsync organizationService)
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

        var request = new RetrieveMultipleRequest()
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<Annotation>()).ToArray();
    }
}
