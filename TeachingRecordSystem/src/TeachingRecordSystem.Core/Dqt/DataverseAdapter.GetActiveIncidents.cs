using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt;

public partial class DataverseAdapter
{
    public async Task<Incident[]> GetActiveIncidents()
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Incident.Fields.StateCode, ConditionOperator.Equal, (int)IncidentState.Active);

        var query = new QueryExpression(Incident.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Incident.Fields.TicketNumber,
                Incident.Fields.Title,
                Incident.Fields.CreatedOn),
            Criteria = filter,
            Orders =
            {
                new OrderExpression(Incident.Fields.CreatedOn, OrderType.Descending)
            }
        };

        AddContactLink(query);
        AddSubjectLink(query);

        var result = await _service.RetrieveMultipleAsync(query);
        return result.Entities.Select(entity => entity.ToEntity<Incident>()).ToArray();

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
