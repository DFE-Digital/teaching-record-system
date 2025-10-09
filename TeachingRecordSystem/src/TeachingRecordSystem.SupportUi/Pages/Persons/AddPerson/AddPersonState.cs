using System.Text.Json.Serialization;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class AddPersonState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.AddPerson,
        typeof(AddPersonState),
        requestDataKeys: [],
        appendUniqueKey: true);

    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public AddPersonFieldState<EmailAddress> EmailAddress { get; set; } = new("", null);
    public AddPersonFieldState<NationalInsuranceNumber> NationalInsuranceNumber { get; set; } = new("", null);
    public Gender? Gender { get; set; }

    public AddPersonReasonOption? Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool Initialized { get; set; }

    [JsonIgnore]
    public bool IsComplete =>
        IsPersonalDetailsComplete &&
        IsCreateReasonComplete;

    [JsonIgnore]
    public bool IsPersonalDetailsComplete =>
        FirstName is not null &&
        LastName is not null &&
        DateOfBirth.HasValue;

    [JsonIgnore]
    public bool IsCreateReasonComplete =>
        Reason.HasValue &&
        (Reason is not AddPersonReasonOption.AnotherReason || ReasonDetail is not null) &&
        Evidence.IsComplete;

    public void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
    }
}
