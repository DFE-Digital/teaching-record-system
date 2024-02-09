using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactWithParentByIdHandler : ICrmQueryHandler<GetContactWithParentById, (Contact, Contact?)>
{
    public async Task<(Contact, Contact?)> Execute(GetContactWithParentById query, IOrganizationServiceAsync organizationService)
    {
        Contact contact = await FetchContact(query.ContactId, resolveMerges: false);
        Contact? parentContact = default(Contact?);

        if (contact.Merged == true)
        {
            parentContact = await FetchContact(contact.MasterId.Id, resolveMerges: true);
        }
        return (contact, parentContact);

        async Task<Contact> FetchContact(Guid id, bool resolveMerges = true)
        {
            var filter = new FilterExpression();
            filter.AddCondition(Contact.PrimaryIdAttribute, ConditionOperator.Equal, id);

            var queryExpression = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                            Contact.Fields.dfeta_TRN,
                            Contact.Fields.FirstName,
                            Contact.Fields.MiddleName,
                            Contact.Fields.LastName,
                            Contact.Fields.EMailAddress1,
                            Contact.Fields.dfeta_NINumber,
                            Contact.Fields.BirthDate,
                            Contact.Fields.Merged,
                            Contact.Fields.MasterId
                    ),
                Criteria = filter
            };

            var request = new RetrieveMultipleRequest()
            {
                Query = queryExpression
            };

            var result = (RetrieveMultipleResponse)await organizationService.ExecuteAsync(request);
            var teacher = result.EntityCollection.Entities.Select(entity => entity.ToEntity<Contact>()).Single();

            if (teacher.Merged == true && resolveMerges == true)
            {
                return await FetchContact(teacher.MasterId.Id, resolveMerges);
            }

            return teacher;
        }
    }
}
