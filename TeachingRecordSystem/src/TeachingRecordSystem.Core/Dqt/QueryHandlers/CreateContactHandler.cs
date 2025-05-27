using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateContactHandler : ICrmQueryHandler<CreateContactQuery, Guid>
{
    public async Task<Guid> ExecuteAsync(CreateContactQuery query, IOrganizationServiceAsync organizationService)
    {
        var contactId = query.ContactId;

        var contact = new Contact()
        {
            Id = contactId,
            FirstName = query.FirstName,
            MiddleName = query.MiddleName,
            LastName = query.LastName,
            BirthDate = query.DateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false),
            GenderCode = query.Gender,
            dfeta_NINumber = query.NationalInsuranceNumber,
            EMailAddress1 = query.EmailAddress,
            dfeta_AllowPiiUpdatesFromRegister = query.AllowPiiUpdates,
            dfeta_TrnRequestID = query.TrnRequestId,
            dfeta_TRN = query.Trn
        };

        if (query.Trn is null)
        {
            // CRM plug-in explodes if TRN is specified but is null
            contact.Attributes.Remove(Contact.Fields.dfeta_TRN);
        }

        return await organizationService.CreateAsync(contact);
    }
}
