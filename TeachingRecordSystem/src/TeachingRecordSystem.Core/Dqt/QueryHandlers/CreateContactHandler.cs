using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateContactHandler : ICrmQueryHandler<CreateContactQuery, Guid>
{
    public async Task<Guid> ExecuteAsync(CreateContactQuery query, IOrganizationServiceAsync organizationService)
    {
        var contactId = query.ContactId;

        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        var serializer = new MessageSerializer();

        var contact = new Contact()
        {
            Id = contactId,
            FirstName = query.FirstName,
            MiddleName = query.MiddleName,
            LastName = query.LastName,
            dfeta_StatedFirstName = query.StatedFirstName,
            dfeta_StatedMiddleName = query.StatedMiddleName,
            dfeta_StatedLastName = query.StatedLastName,
            BirthDate = query.DateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false),
            GenderCode = query.Gender,
            dfeta_NINumber = query.NationalInsuranceNumber,
            EMailAddress1 = query.EmailAddress,
            dfeta_AllowPiiUpdatesFromRegister = query.AllowPiiUpdates,
            dfeta_TrnRequestID = query.TrnRequestId,
            dfeta_TRN = query.Trn,
        };

        if (query.Trn is null)
        {
            // CRM plug-in explodes if TRN is specified but is null
            contact.Attributes.Remove(Contact.Fields.dfeta_TRN);
        }

        requestBuilder.AddRequest(new CreateRequest() { Target = contact });

        foreach (var reviewTask in query.ReviewTasks)
        {
            var task = new CrmTask()
            {
                RegardingObjectId = contactId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_potentialduplicateid = reviewTask.PotentialDuplicateContactId.ToEntityReference(Contact.EntityLogicalName),
                Category = reviewTask.Category,
                Subject = reviewTask.Subject,
                Description = reviewTask.Description
            };

            requestBuilder.AddRequest(new CreateRequest() { Target = task });
        }

        requestBuilder.AddRequest(new CreateRequest() { Target = serializer.CreateCrmOutboxMessage(query.TrnRequestMetadataMessage) });

        await requestBuilder.ExecuteAsync();

        return contactId;


    }
}
