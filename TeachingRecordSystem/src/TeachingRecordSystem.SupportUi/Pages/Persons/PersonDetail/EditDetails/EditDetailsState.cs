using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public class EditDetailsState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditDetails,
        typeof(EditDetailsState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public EditDetailsFieldState<EmailAddress> EmailAddress { get; set; }
    public EditDetailsFieldState<MobileNumber> MobileNumber { get; set; }
    public EditDetailsFieldState<NationalInsuranceNumber> NationalInsuranceNumber { get; set; }
    public EditDetailsChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }

    public bool Initialized { get; set; }

    [JsonIgnore]
    public bool IsComplete =>
        FirstName is not null &&
        LastName is not null &&
        DateOfBirth.HasValue &&
        ChangeReason.HasValue &&
        (ChangeReason.Value is not EditDetailsChangeReasonOption.AnotherReason || ChangeReasonDetail is not null) &&
        UploadEvidence.HasValue &&
        (UploadEvidence.Value is not true || EvidenceFileId.HasValue);

    public async Task EnsureInitializedAsync(TrsDbContext dbContext, Guid personId)
    {
        if (Initialized)
        {
            return;
        }

        var person = await dbContext.Persons.SingleAsync(q => q.PersonId == personId);

        FirstName = person.FirstName;
        MiddleName = person.MiddleName;
        LastName = person.LastName;
        DateOfBirth = person.DateOfBirth;
        EmailAddress = new EditDetailsFieldState<EmailAddress>(person.EmailAddress, Core.EmailAddress.TryParse(person.EmailAddress, out var emailAddress) ? emailAddress : null);
        MobileNumber = new EditDetailsFieldState<MobileNumber>(person.MobileNumber, Core.MobileNumber.TryParse(person.MobileNumber, out var mobileNumber) ? mobileNumber : null);
        NationalInsuranceNumber = new EditDetailsFieldState<NationalInsuranceNumber>(person.NationalInsuranceNumber, Core.NationalInsuranceNumber.TryParse(person.NationalInsuranceNumber, out var nationalInsuranceNumber) ? nationalInsuranceNumber : null);

        Initialized = true;
    }
}

public record EditDetailsFieldState<T>(string? Raw, T? Parsed);
//{
//    public string? Raw { get; set; }
//    public T? Parsed { get; set; }
//}
