using System.Diagnostics;
using System.Text;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateContactHandler : ICrmQueryHandler<CreateContactQuery, Guid>
{
    public async Task<Guid> Execute(CreateContactQuery query, IOrganizationServiceAsync organizationService)
    {
        var contactId = Guid.NewGuid();

        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);

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
            dfeta_NINumber = query.NationalInsuranceNumber,
            EMailAddress1 = query.EmailAddress,
            dfeta_AllowPiiUpdatesFromRegister = false,
            dfeta_TrnRequestID = query.TrnRequestId,
            dfeta_TRN = query.Trn
        };

        requestBuilder.AddRequest(new CreateRequest() { Target = contact });

        foreach (var (duplicate, hasActiveAlert) in query.PotentialDuplicates)
        {
            var task = CreateDuplicateReviewTaskEntity(duplicate, contactId, hasActiveAlert);
            requestBuilder.AddRequest(new CreateRequest() { Target = task });
        }

        await requestBuilder.Execute();

        return contactId;
    }

    private CrmTask CreateDuplicateReviewTaskEntity(FindPotentialDuplicateContactsResult duplicate, Guid contactId, bool hasActiveAlert)
    {
        var description = GetDescription();

        var category = "DMSImportTrn";

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
