using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public class EditDetailsState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditDetails,
        typeof(EditDetailsState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public string OriginalFirstName { get; set; } = "";
    public string OriginalMiddleName { get; set; } = "";
    public string OriginalLastName { get; set; } = "";
    public DateOnly? OriginalDateOfBirth { get; set; }
    public EditDetailsFieldState<EmailAddress> OriginalEmailAddress { get; set; } = new("", null);
    public EditDetailsFieldState<NationalInsuranceNumber> OriginalNationalInsuranceNumber { get; set; } = new("", null);
    public Gender? OriginalGender { get; set; }

    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public EditDetailsFieldState<EmailAddress> EmailAddress { get; set; } = new("", null);
    public EditDetailsFieldState<NationalInsuranceNumber> NationalInsuranceNumber { get; set; } = new("", null);
    public Gender? Gender { get; set; }

    public EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    public EvidenceUploadModel NameChangeEvidence { get; set; } = new();

    public EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    public string? OtherDetailsChangeReasonDetail { get; set; }
    public EvidenceUploadModel OtherDetailsChangeEvidence { get; set; } = new();

    public bool Initialized { get; set; }

    [JsonIgnore]
    public bool NameChanged =>
        FirstName != OriginalFirstName ||
        MiddleName != OriginalMiddleName ||
        LastName != OriginalLastName;

    [JsonIgnore]
    public bool OtherDetailsChanged =>
        DateOfBirth != OriginalDateOfBirth ||
        EmailAddress != OriginalEmailAddress ||
        NationalInsuranceNumber != OriginalNationalInsuranceNumber ||
        Gender != OriginalGender;

    [JsonIgnore]
    public bool IsComplete =>
        IsPersonalDetailsComplete &&
        IsNameChangeReasonComplete &&
        IsOtherDetailsChangeReasonComplete;

    [JsonIgnore]
    public bool IsPersonalDetailsComplete =>
        FirstName is not null &&
        LastName is not null &&
        DateOfBirth.HasValue &&
        (NameChanged || OtherDetailsChanged);

    [JsonIgnore]
    public bool IsNameChangeReasonComplete =>
        !NameChanged ||
            (NameChangeReason.HasValue &&
            NameChangeEvidence.IsComplete);

    [JsonIgnore]
    public bool IsOtherDetailsChangeReasonComplete =>
        !OtherDetailsChanged ||
            (OtherDetailsChangeReason.HasValue &&
            (OtherDetailsChangeReason.Value is not EditDetailsOtherDetailsChangeReasonOption.AnotherReason || OtherDetailsChangeReasonDetail is not null) &&
            OtherDetailsChangeEvidence.IsComplete);

    public void EnsureInitialized(Person person)
    {
        if (Initialized)
        {
            return;
        }

        OriginalFirstName = person.FirstName;
        OriginalMiddleName = person.MiddleName;
        OriginalLastName = person.LastName;
        OriginalDateOfBirth = person.DateOfBirth;
        OriginalEmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(person.EmailAddress);
        OriginalNationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(person.NationalInsuranceNumber);
        OriginalGender = person.Gender;

        FirstName = OriginalFirstName;
        MiddleName = OriginalMiddleName;
        LastName = OriginalLastName;
        DateOfBirth = OriginalDateOfBirth;
        EmailAddress = OriginalEmailAddress;
        NationalInsuranceNumber = OriginalNationalInsuranceNumber;
        Gender = OriginalGender;

        Initialized = true;
    }
}
