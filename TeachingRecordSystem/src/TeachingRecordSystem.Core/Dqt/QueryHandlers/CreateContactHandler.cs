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
            dfeta_AllowPiiUpdatesFromRegister = false
        };

        // only set trn if there is not any potential duplicate matches on the query
        if (query.PotentialDuplicates.Length == 0)
        {
            contact.dfeta_TRN = query.Trn;
        }

        requestBuilder.AddRequest(new CreateRequest() { Target = contact });

        if (query.PotentialDuplicates.Length == 0)
        {
            FlagBadData(requestBuilder, query, contactId);
        }
        else
        {
            foreach (var duplicate in query.PotentialDuplicates)
            {
                var task = CreateDuplicateReviewTaskEntity(duplicate, query, contactId);
                requestBuilder.AddRequest(new CreateRequest() { Target = task });
            }
        }

        await requestBuilder.Execute();

        return contactId;
    }

    private void FlagBadData(RequestBuilder requestBuilder, CreateContactQuery createTeacherRequest, Guid contactId)
    {
        var firstNameContainsDigit = createTeacherRequest.FirstName.Any(Char.IsDigit);
        var middleNameContainsDigit = createTeacherRequest.MiddleName?.Any(Char.IsDigit) ?? false;
        var lastNameContainsDigit = createTeacherRequest.LastName.Any(Char.IsDigit);

        if (firstNameContainsDigit || middleNameContainsDigit || lastNameContainsDigit)
        {
            requestBuilder.AddRequest(new CreateRequest()
            {
                Target = CreateNameWithDigitsReviewTaskEntity(firstNameContainsDigit, middleNameContainsDigit, lastNameContainsDigit, contactId)
            });
        }
    }

    private CrmTask CreateDuplicateReviewTaskEntity(FindPotentialDuplicateContactsResult duplicate, CreateContactQuery createTeacherRequest, Guid contactId)
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

            foreach (var matchedAttribute in duplicate.MatchedAttributes ?? Array.Empty<string>())
            {
                sb.AppendLine(matchedAttribute switch
                {
                    Contact.Fields.FirstName => $"  - First name: '{duplicate.FirstName}'",
                    Contact.Fields.MiddleName => $"  - Middle name: '{duplicate.MiddleName}'",
                    Contact.Fields.LastName => $"  - Last name: '{duplicate.LastName}'",
                    Contact.Fields.BirthDate => $"  - Date of birth: '{duplicate.DateOfBirth:dd/MM/yyyy}'",
                    Contact.Fields.EMailAddress1 => $"  - Email address: '{duplicate.EmailAddress}'",
                    _ => throw new Exception($"Unknown matched field: '{matchedAttribute}'.")
                });
            }

            Debug.Assert(!duplicate.HasEytsDate || !duplicate.HasQtsDate);
            var additionalFlags = new List<string>();

            if (duplicate.HasActiveSanctions)
            {
                additionalFlags.Add("active sanctions");
            }

            if (duplicate.HasQtsDate)
            {
                additionalFlags.Add("QTS date");
            }
            else if (duplicate.HasEytsDate)
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

    private CrmTask CreateNameWithDigitsReviewTaskEntity(
        bool firstNameContainsDigit,
        bool middleNameContainsDigit,
        bool lastNameContainsDigit,
        Guid teacherId)
    {
        var description = GetDescription();

        return new CrmTask()
        {
            RegardingObjectId = teacherId.ToEntityReference(Contact.EntityLogicalName),
            Category = "DMSImportTrn",
            Subject = "Notification for QTS Unit Team",
            Description = description
        };

        string GetDescription()
        {
            var badFields = new List<string>();

            if (firstNameContainsDigit)
            {
                badFields.Add("first name");
            }

            if (middleNameContainsDigit)
            {
                badFields.Add("middle name");
            }

            if (lastNameContainsDigit)
            {
                badFields.Add("last name");
            }

            Debug.Assert(badFields.Count > 0);

            var description = badFields.ToCommaSeparatedString(finalValuesConjunction: "and")
                + $" contain{(badFields.Count == 1 ? "s" : "")} a digit";

            description = description[0..1].ToUpper() + description[1..];

            return description;
        }
    }
}
