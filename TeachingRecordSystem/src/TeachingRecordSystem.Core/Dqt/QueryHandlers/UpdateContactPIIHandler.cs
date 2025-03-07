using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;


public class UpdateContactPIIHandler : ICrmQueryHandler<UpdateContactPIIQuery, bool>
{
    public async Task<bool> ExecuteAsync(UpdateContactPIIQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.ContactId,
                FirstName = query.FirstName,
                MiddleName = query.MiddleName,
                LastName = query.LastName,
                //GenderCode = q
            }
        });

        return true;
    }
}
