using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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

    public string? OriginalFirstName { get; set; }
    public string? OriginalMiddleName { get; set; }
    public string? OriginalLastName { get; set; }
    public DateOnly? OriginalDateOfBirth { get; set; }
    public EditDetailsFieldState<EmailAddress> OriginalEmailAddress { get; set; } = new(null, null);
    public EditDetailsFieldState<MobileNumber> OriginalMobileNumber { get; set; } = new(null, null);
    public EditDetailsFieldState<NationalInsuranceNumber> OriginalNationalInsuranceNumber { get; set; } = new(null, null);

    public EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    public bool? NameChangeUploadEvidence { get; set; }
    public Guid? NameChangeEvidenceFileId { get; set; }
    public string? NameChangeEvidenceFileName { get; set; }
    public string? NameChangeEvidenceFileSizeDescription { get; set; }

    public EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    public string? OtherDetailsChangeReasonDetail { get; set; }
    public bool? OtherDetailsChangeUploadEvidence { get; set; }
    public Guid? OtherDetailsChangeEvidenceFileId { get; set; }
    public string? OtherDetailsChangeEvidenceFileName { get; set; }
    public string? OtherDetailsChangeEvidenceFileSizeDescription { get; set; }

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
        MobileNumber != OriginalMobileNumber ||
        NationalInsuranceNumber != OriginalNationalInsuranceNumber;

    [JsonIgnore]
    public bool IsIndexComplete =>
        FirstName is not null &&
        LastName is not null &&
        DateOfBirth.HasValue &&
        (NameChanged || OtherDetailsChanged);

    [JsonIgnore]
    public bool IsNameChangeReasonComplete =>
        !NameChanged ||
            NameChangeReason.HasValue &&
            NameChangeUploadEvidence.HasValue &&
            NameChangeUploadEvidence.Value is not true || NameChangeEvidenceFileId.HasValue;

    [JsonIgnore]
    public bool IsOtherDetailsChangeReasonComplete =>
        !OtherDetailsChanged ||
            OtherDetailsChangeReason.HasValue &&
            (OtherDetailsChangeReason.Value is not EditDetailsOtherDetailsChangeReasonOption.AnotherReason || OtherDetailsChangeReasonDetail is not null) &&
            OtherDetailsChangeUploadEvidence.HasValue &&
            (OtherDetailsChangeUploadEvidence.Value is not true || OtherDetailsChangeEvidenceFileId.HasValue);

    [JsonIgnore]
    public bool IsComplete =>
        IsIndexComplete &&
        IsNameChangeReasonComplete &&
        IsOtherDetailsChangeReasonComplete;

    public void EnsureInitialized(Person person)
    {
        if (Initialized)
        {
            return;
        }

        FirstName = person.FirstName;
        MiddleName = person.MiddleName;
        LastName = person.LastName;
        DateOfBirth = person.DateOfBirth;
        EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(person.EmailAddress);
        MobileNumber = EditDetailsFieldState<MobileNumber>.FromRawValue(person.MobileNumber);
        NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(person.NationalInsuranceNumber);

        OriginalFirstName = FirstName;
        OriginalMiddleName = MiddleName;
        OriginalLastName = LastName;
        OriginalDateOfBirth = DateOfBirth;
        OriginalEmailAddress = EmailAddress;
        OriginalMobileNumber = MobileNumber;
        OriginalNationalInsuranceNumber = NationalInsuranceNumber;

        Initialized = true;
    }
}
