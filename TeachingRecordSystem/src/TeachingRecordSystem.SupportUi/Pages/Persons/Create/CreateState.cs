using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Create;

public class CreateState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.CreatePerson,
        typeof(CreateState),
        requestDataKeys: [],
        appendUniqueKey: true);

    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public CreateFieldState<EmailAddress> EmailAddress { get; set; } = new("", null);
    public CreateFieldState<MobileNumber> MobileNumber { get; set; } = new("", null);
    public CreateFieldState<NationalInsuranceNumber> NationalInsuranceNumber { get; set; } = new("", null);
    public Gender? Gender { get; set; }

    public CreateReasonOption? CreateReason { get; set; }
    public string? CreateReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }

    public bool Initialized { get; set; }

    [JsonIgnore]
    public bool IsPersonalDetailsComplete =>
        FirstName is not null &&
        LastName is not null &&
        DateOfBirth.HasValue;

    [JsonIgnore]
    public bool IsCreateReasonComplete =>
        CreateReason.HasValue &&
        (CreateReason.Value is not CreateReasonOption.AnotherReason || CreateReasonDetail is not null) &&
        UploadEvidence.HasValue &&
        (UploadEvidence.Value is not true || EvidenceFileId.HasValue);

    [JsonIgnore]
    public bool IsComplete =>
        IsPersonalDetailsComplete &&
        IsCreateReasonComplete;

    public void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
    }
}
