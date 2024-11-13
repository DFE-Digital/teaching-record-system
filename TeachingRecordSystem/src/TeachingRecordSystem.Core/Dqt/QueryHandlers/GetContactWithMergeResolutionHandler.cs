using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactWithMergeResolutionHandler : ICrmQueryHandler<GetContactWithMergeResolutionQuery, Contact>
{
    public async Task<Contact> Execute(GetContactWithMergeResolutionQuery query, IOrganizationServiceAsync organizationService)
    {
        // Ensure we have MasterId in the columns requested
        var columns = query.ColumnSet.AllColumns ? query.ColumnSet :
            new ColumnSet([Contact.Fields.MasterId, .. query.ColumnSet.Columns]);

        return await FetchContact(query.ContactId);

        async Task<Contact> FetchContact(Guid id)
        {
            var filter = new FilterExpression();
            filter.AddCondition(Contact.PrimaryIdAttribute, ConditionOperator.Equal, id);

            var queryExpression = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = columns,
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
