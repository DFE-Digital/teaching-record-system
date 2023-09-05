using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactDetailByIdHandler : ICrmQueryHandler<GetContactDetailByIdQuery, ContactDetail?>
{
    public async Task<ContactDetail?> Execute(GetContactDetailByIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(Contact.PrimaryIdAttribute, ConditionOperator.Equal, query.ContactId);

        var queryExpression = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
            Criteria = filter
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        var contact = response.Entities.SingleOrDefault()?.ToEntity<Contact>();
        if (contact is null)
        {
            return null;
        }

        return new ContactDetail(contact);
    }
}
