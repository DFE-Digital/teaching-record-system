using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateContactPiiHandler : ICrmQueryHandler<UpdateContactPiiQuery, bool>
{
    public async Task<bool> ExecuteAsync(UpdateContactPiiQuery query, IOrganizationServiceAsync organizationService)
    {
        var updateRequest = new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.ContactId,
                FirstName = query.FirstName,
                MiddleName = query.MiddleName,
                LastName = query.LastName,
                dfeta_NINumber = query.NationalInsuranceNumber,
                BirthDate = query.DateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false),
                GenderCode = query.Gender,
                EMailAddress1 = query.EmailAddress
            }
        };
        updateRequest.Parameters.Add("tag", "AllowRegisterPiiUpdates");

        await organizationService.ExecuteAsync(updateRequest);

        return true;
    }
}
