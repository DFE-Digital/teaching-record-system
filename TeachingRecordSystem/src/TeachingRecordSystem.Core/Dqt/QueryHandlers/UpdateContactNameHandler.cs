using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateContactNameHandler : ICrmQueryHandler<UpdateContactNameQuery, bool>
{
    public async Task<bool> Execute(UpdateContactNameQuery query, IOrganizationServiceAsync organizationService)
    {
        await organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.ContactId,
                FirstName = query.FirstName,
                MiddleName = query.MiddleName,
                LastName = query.LastName
            }
        });

        return true;
    }
}
