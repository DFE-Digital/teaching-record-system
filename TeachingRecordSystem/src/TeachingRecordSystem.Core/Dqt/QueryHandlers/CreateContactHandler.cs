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
        var TeacherId = Guid.NewGuid();

        // Send a single Transaction request with all the data changes in.
        // This is important for atomicity; we really do not want torn writes here.
        var txnRequest = new ExecuteTransactionRequest()
        {
            ReturnResponses = true,
            Requests = new()
        };

        var contact = new Contact()
        {
            Id = TeacherId,
            FirstName = query.FirstName,
            MiddleName = query.MiddleName,
            LastName = query.LastName,
            BirthDate = query.DateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false),
            dfeta_NINumber = query.NationalInsuranceNumber,
            EMailAddress1 = query.Email,
            dfeta_AllowPiiUpdatesFromRegister = true
        };

        // only set trn if there is not any potential duplicate matches on the query
        if (query.ExistingTeacherResult is null)
        {
            contact.dfeta_TRN = query.Trn;
        }

        txnRequest.Requests.Add(new CreateRequest() { Target = contact });
        if (query.ExistingTeacherResult is null)
        {
            FlagBadData(txnRequest, query, TeacherId);
        }
        else
        {
            //add duplicate review task
            var task = CreateDuplicateReviewTaskEntity(query.ExistingTeacherResult, query, TeacherId);
            txnRequest.Requests.Add(new CreateRequest() { Target = task });
        }

        await organizationService.ExecuteAsync(txnRequest);
        return TeacherId;
    }

    public void FlagBadData(ExecuteTransactionRequest txnRequest, CreateContactQuery createTeacherRequest, Guid teacherId)
    {
        var firstNameContainsDigit = createTeacherRequest.FirstName.Any(Char.IsDigit);
        var middleNameContainsDigit = createTeacherRequest.MiddleName?.Any(Char.IsDigit) ?? false;
        var lastNameContainsDigit = createTeacherRequest.LastName.Any(Char.IsDigit);

        if (firstNameContainsDigit || middleNameContainsDigit || lastNameContainsDigit)
        {
            txnRequest.Requests.Add(new CreateRequest()
            {
                Target = CreateNameWithDigitsReviewTaskEntity(firstNameContainsDigit, middleNameContainsDigit, lastNameContainsDigit, teacherId)
            });
        }
    }

    public CrmTask CreateDuplicateReviewTaskEntity(FindExistingTrnResult duplicate, CreateContactQuery createTeacherRequest, Guid TeacherId)
    {
        var description = GetDescription();

        var category = "DMSImportTrn";

        return new CrmTask()
        {
            RegardingObjectId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_potentialduplicateid = duplicate.TeacherId.ToEntityReference(Contact.EntityLogicalName),
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
                    Contact.Fields.FirstName => $"  - First name: '{createTeacherRequest.FirstName}'",
                    Contact.Fields.MiddleName => $"  - Middle name: '{createTeacherRequest.MiddleName}'",
                    Contact.Fields.LastName => $"  - Last name: '{createTeacherRequest.LastName}'",
                    Contact.Fields.BirthDate => $"  - Date of birth: '{createTeacherRequest.DateOfBirth:dd/MM/yyyy}'",
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

    public Models.Task CreateNameWithDigitsReviewTaskEntity(
          bool firstNameContainsDigit,
          bool middleNameContainsDigit,
          bool lastNameContainsDigit,
          Guid teacherId)
    {
        var description = GetDescription();

        return new Models.Task()
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
