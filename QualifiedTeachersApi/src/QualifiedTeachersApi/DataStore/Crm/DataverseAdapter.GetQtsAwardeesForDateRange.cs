using Microsoft.Xrm.Sdk.Query;
using QualifiedTeachersApi.DataStore.Crm.Models;

namespace QualifiedTeachersApi.DataStore.Crm;

public partial class DataverseAdapter
{
    public async Task<QtsAwardee[]> GetQtsAwardeesForDateRange(DateTime startDate, DateTime endDate)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

        var columnNames = new[]
        {
            Contact.Fields.dfeta_TRN,
            Contact.Fields.FirstName,
            Contact.Fields.LastName,
            Contact.Fields.EMailAddress1,
            Contact.Fields.EMailAddress2
        };

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new(columnNames),
            Criteria = filter
        };

        var result = await _service.RetrieveMultipleAsync(query);
        return result.Entities
            .Select(e => e.ToEntity<Contact>())
            .Select(MapContactToQtsAwardee)
            .ToArray();
    }

    private QtsAwardee MapContactToQtsAwardee(Contact contact)
    {
        var useStatedNames = !string.IsNullOrEmpty(contact.dfeta_StatedFirstName) && !string.IsNullOrEmpty(contact.dfeta_StatedLastName);

        return new QtsAwardee
        {
            TeacherId = contact.Id,
            Trn = contact.dfeta_TRN,
            FirstName = useStatedNames ? contact.dfeta_StatedFirstName : contact.FirstName,
            LastName = useStatedNames ? contact.dfeta_StatedLastName : contact.LastName,
            EmailAddress = contact.EMailAddress1
        };
    }
}
