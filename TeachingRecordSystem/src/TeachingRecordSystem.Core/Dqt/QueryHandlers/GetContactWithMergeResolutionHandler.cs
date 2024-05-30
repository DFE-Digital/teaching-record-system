using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactWithMergeResolutionHandler : ICrmQueryHandler<GetContactWithMergeResolutionQuery, Contact>
{
    public async Task<Contact> Execute(GetContactWithMergeResolutionQuery query, IOrganizationServiceAsync organizationService)
    {
        return await FetchContact(query.ContactId);

        async Task<Contact> FetchContact(Guid id)
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
                    Contact.Fields.MasterId),
                Criteria = filter
            };

            var request = new RetrieveMultipleRequest()
            {
                Query = queryExpression
            };

            var result = (RetrieveMultipleResponse)await organizationService.ExecuteAsync(request);
            var teacher = result.EntityCollection.Entities.Select(entity => entity.ToEntity<Contact>()).Single();

            if (teacher.Merged == true)
            {
                return await FetchContact(teacher.MasterId.Id);
            }

            return teacher;
        }
    }
}
