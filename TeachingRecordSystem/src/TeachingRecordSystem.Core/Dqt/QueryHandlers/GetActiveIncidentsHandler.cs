using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveIncidentsHandler : ICrmQueryHandler<GetActiveIncidentsQuery, GetIncidentsResult>
{
    public async Task<GetIncidentsResult> Execute(GetActiveIncidentsQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Incident.Fields.StateCode, ConditionOperator.Equal, (int)IncidentState.Active);

        var queryExpression = new QueryExpression(Incident.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Incident.Fields.TicketNumber,
                Incident.Fields.Title,
                Incident.Fields.CreatedOn),
            Criteria = filter,
            Orders =
            {
                new OrderExpression(Incident.Fields.CreatedOn, OrderType.Ascending)
            },
            PageInfo = new()
            {
                PageNumber = query.PageNumber,
                Count = query.PageSize,
                ReturnTotalRecordCount = true
            }
        };

        AddContactLink(queryExpression);
        AddSubjectLink(queryExpression);

        var result = await organizationService.RetrieveMultipleAsync(queryExpression);
        var incidents = result.Entities.Select(entity => entity.ToEntity<Incident>()).ToArray();

        return new GetIncidentsResult(incidents, result.TotalRecordCount);

        static void AddContactLink(QueryExpression query)
        {
            var contactLink = query.AddLink(
                Contact.EntityLogicalName,
                Incident.Fields.CustomerId,
                Contact.PrimaryIdAttribute,
                JoinOperator.Inner);

            contactLink.Columns = new ColumnSet(
                Contact.PrimaryIdAttribute,
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedLastName);

            contactLink.EntityAlias = Contact.EntityLogicalName;
        }

        static void AddSubjectLink(QueryExpression query)
        {
            var subjectLink = query.AddLink(
                Subject.EntityLogicalName,
                Incident.Fields.SubjectId,
                Subject.PrimaryIdAttribute,
                JoinOperator.Inner);

            subjectLink.Columns = new ColumnSet(
                Subject.PrimaryIdAttribute,
                Subject.Fields.Title);

            subjectLink.EntityAlias = Subject.EntityLogicalName;
        }
    }
}
