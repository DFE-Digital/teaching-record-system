using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateContactDateOfBirthHandler : ICrmQueryHandler<UpdateContactDateOfBirthQuery, bool>
{
    public async Task<bool> Execute(UpdateContactDateOfBirthQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.ContactId,
                BirthDate = query.DateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false)
            }
        });

        return true;
    }
}
