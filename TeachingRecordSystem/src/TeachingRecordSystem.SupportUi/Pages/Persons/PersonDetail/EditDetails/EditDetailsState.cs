using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Views.Shared.Components.UploadEvidence;

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
    public EditDetailsFieldState<EmailAddress> EmailAddress { get; set; } = new(null, null);
    public EditDetailsFieldState<MobileNumber> MobileNumber { get; set; } = new(null, null);
    public EditDetailsFieldState<NationalInsuranceNumber> NationalInsuranceNumber { get; set; } = new(null, null);
    public EditDetailsChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public UploadEvidenceViewModel? UploadEvidence { get; set; }

    public bool Initialized { get; set; }

    [JsonIgnore]
    public bool IsComplete =>
        FirstName is not null &&
        LastName is not null &&
        DateOfBirth.HasValue &&
        ChangeReason.HasValue &&
        (ChangeReason.Value is not EditDetailsChangeReasonOption.AnotherReason || ChangeReasonDetail is not null) &&
        UploadEvidence is not null && UploadEvidence.IsValid;

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
        EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(person.EmailAddress);
        MobileNumber = EditDetailsFieldState<MobileNumber>.FromRawValue(person.MobileNumber);
        NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(person.NationalInsuranceNumber);

        Initialized = true;
    }
}
