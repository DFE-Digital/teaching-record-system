using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

public class CreateReviewTaskHandler : ICrmQueryHandler<CreateReviewTaskQuery, Guid>
{
    public async Task<Guid> Execute(CreateReviewTaskQuery query, IOrganizationServiceAsync organizationService)
    {
        var crmTaskId = await organizationService.CreateAsync(new CrmTask()
        {
            RegardingObjectId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
            Category = query.Category,
            Subject = query.Subject,
            Description = query.Description
        });

        return crmTaskId;
    }
}
