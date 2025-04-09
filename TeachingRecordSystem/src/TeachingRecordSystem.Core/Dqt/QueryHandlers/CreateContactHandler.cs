using System.Diagnostics;
using System.Text;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateContactHandler : ICrmQueryHandler<CreateContactQuery, Guid>
{
    public async Task<Guid> ExecuteAsync(CreateContactQuery query, IOrganizationServiceAsync organizationService)
    {
        var contactId = Guid.NewGuid();

        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        var serializer = new MessageSerializer();

        Debug.Assert(query.Trn is null || query.PotentialDuplicates.Count == 0);

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

        foreach (var (duplicate, hasActiveAlert) in query.PotentialDuplicates)
        {
            var task = CreateDuplicateReviewTaskEntity(duplicate, hasActiveAlert);
            requestBuilder.AddRequest(new CreateRequest() { Target = task });
        }

        foreach (var outboxMessage in query.OutboxMessages)
        {
            requestBuilder.AddRequest(new CreateRequest() { Target = serializer.CreateCrmOutboxMessage(outboxMessage) });
        }

        await requestBuilder.ExecuteAsync();

        return contactId;

        CrmTask CreateDuplicateReviewTaskEntity(FindPotentialDuplicateContactsResult duplicate, bool hasActiveAlert)
        {
            var description = GetDescription();

            var category = $"TRN request from {query.ApplicationUserName}";

            return new CrmTask()
            {
                RegardingObjectId = contactId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_potentialduplicateid = duplicate.ContactId.ToEntityReference(Contact.EntityLogicalName),
                Category = category,
                Subject = "Notification for QTS Unit Team",
                Description = description
            };

            string GetDescription()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Potential duplicate");
                sb.AppendLine("Matched on");

                foreach (var matchedAttribute in duplicate.MatchedAttributes)
                {
                    sb.AppendLine(matchedAttribute switch
                    {
                        Contact.Fields.FirstName => $"  - First name: '{duplicate.FirstName}'",
                        Contact.Fields.MiddleName => $"  - Middle name: '{duplicate.MiddleName}'",
                        Contact.Fields.LastName => $"  - Last name: '{duplicate.LastName}'",
                        Contact.Fields.dfeta_PreviousLastName => $"  - Previous last name: '{duplicate.PreviousLastName}'",
                        Contact.Fields.BirthDate => $"  - Date of birth: '{duplicate.DateOfBirth:dd/MM/yyyy}'",
                        Contact.Fields.dfeta_NINumber => $"  - National Insurance number: '{duplicate.NationalInsuranceNumber}'",
                        Contact.Fields.EMailAddress1 => $"  - Email address: '{duplicate.EmailAddress}'",
                        _ => throw new Exception($"Unknown matched field: '{matchedAttribute}'.")
                    });
                }

                var additionalFlags = new List<string>();

                if (hasActiveAlert)
                {
                    additionalFlags.Add("active sanctions");
                }

                if (duplicate.HasQtsDate)
                {
                    additionalFlags.Add("QTS date");
                }

                if (duplicate.HasEytsDate)
                {
                    additionalFlags.Add("EYTS date");
                }

                if (additionalFlags.Count > 0)
                {
                    sb.AppendLine($"Matched record has {string.Join(" & ", additionalFlags)}");
                }

                return sb.ToString();
            }
        }
    }
}
